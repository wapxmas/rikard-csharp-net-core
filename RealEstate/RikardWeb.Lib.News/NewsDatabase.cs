using RikardWeb.Lib.Db;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using RikardWeb.Lib.News.DbModels;
using System.Threading.Tasks;
using RikardWeb.Lib.News.Data;
using System.Linq;
using RikardLib.Text;
using RikardLib.MyStem;
using RikardLib.Log;
using RikardLib.Maths;
using MongoDB.Bson;

namespace RikardWeb.Lib.News
{
    public class NewsDatabase
    {
        private static volatile object _instanceLock = new object();

        private static NewsDatabase _instance;

        public static NewsDatabase Instance
        {
            get
            {
                return _instance;
            }
        }

        private const string NEWS_COLLECTION_NAME = "news";
        private const string NEWS_WORD_WEIGHTS_COLLECTION_NAME = "stat.weights";

        private const int SIMILAR_NEWS_WINDOW = 300;

        private readonly Logger logger = new Logger();

        private readonly DatabaseFactory databaseFactory;
        private readonly IMongoDatabase newsDatabase;

        public IMongoCollection<NewsDbModel> NewsCollection { get; private set; }
        public IMongoCollection<WordWeightDbModel> WwCollection { get; private set; }

        private NewsDatabase(DatabaseFactory databaseFactory)
        {
            this.databaseFactory = databaseFactory;
            this.newsDatabase = databaseFactory.MongoDatabase;

            this.NewsCollection = newsDatabase.GetCollection<NewsDbModel>(NEWS_COLLECTION_NAME);
            this.WwCollection = newsDatabase.GetCollection<WordWeightDbModel>(NEWS_WORD_WEIGHTS_COLLECTION_NAME);

            /*
             * Creating necessary indexes
            */
            NewsCollection.Indexes.CreateOne(Builders<NewsDbModel>.IndexKeys.Ascending(_ => _.Guid), new CreateIndexOptions { Unique = true });
            NewsCollection.Indexes.CreateOne(Builders<NewsDbModel>.IndexKeys.Ascending(_ => _.Date));
            NewsCollection.Indexes.CreateOne(Builders<NewsDbModel>.IndexKeys.Ascending(_ => _.Url));
        }

        public async Task UpdateNewsTweet(string guid, string tweetId, DateTime date)
        {
            var filter = Builders<NewsDbModel>.Filter.Eq(_ => _.Guid, guid);
            var update = Builders<NewsDbModel>.Update.Set(_ => _.Tweet, new Tweet { TweetId = tweetId, Date = date });
            await NewsCollection.UpdateOneAsync(filter, update);
        }

        public async Task<NewsDbModel> GetLastTweet()
        {
            return (await NewsCollection.Find(_ => _.Tweet != null).SortByDescending(_ => _.Date).Limit(1).ToListAsync()).FirstOrDefault();
        }

        public async Task<IEnumerable<NewsDbModel>> GetNewsNoTweet(int limit)
        {
            var sortOrderOption = Builders<NewsDbModel>.Sort.Ascending(_ => _.Date).Ascending(_ => _.Id);
            return await NewsCollection.Find(_ => _.Tweet == null).Sort(sortOrderOption).Limit(limit).ToListAsync();
        }

        public async Task<long> GetTotalNewsNumber()
        {
            return await NewsCollection.CountAsync(new BsonDocument());
        }

        public async Task<NewsDbModel> GetNewsArticleByGuid(string guid)
        {
            return (await NewsCollection.Find(_ => _.Guid == guid).SortByDescending(_ => _.Date).ToListAsync()).FirstOrDefault();
        }

        public async Task<IEnumerable<NewsDbModel>> GetLatestNewsArticles(int count, int skip)
        {
            return await NewsCollection.Find(_ => true).SortByDescending(_ => _.Date).Skip(skip).Limit(count).ToListAsync();
        }

        public async Task FillSimilarNews()
        {
            var weightsTemplate = (await WwCollection.Find(_ => true).ToListAsync()).ToDictionary(_ => _.Word, __ => (double)0);
            var news = new List<SimilarArticle>();

            foreach (var n in await NewsCollection.Find(_ => _.WW != null).SortBy(_ => _.Date).ToListAsync())
            {
                if (news.Count > 0 && n.Similar == null)
                {
                    var docWeight = FillWeights(n.WW);
                    var similar = new List<SimilarArticle>();

                    foreach (var n2 in news)
                    {
                        if (n2.Weights == null)
                        {
                            n2.Weights = FillWeights(n2.Article.WW);
                        }
                        similar.Add(new SimilarArticle
                        {
                            Article = n2.Article,
                            Similarity = MathUtils.CosineSimilarity(docWeight, n2.Weights),
                            Guid = n2.Article.Guid
                        });
                    }

                    var similarNews = similar.OrderByDescending(_ => _.Similarity).Take(10).ToList();

                    var filter = Builders<NewsDbModel>.Filter.Eq(_ => _.Guid, n.Guid);
                    var update = Builders<NewsDbModel>.Update.Set(_ => _.Similar, similarNews);
                    await NewsCollection.UpdateOneAsync(filter, update);

                    logger.Info($"Added similar news for {n.Guid}");
                }

                news.Add(new SimilarArticle
                {
                    Article = n
                });

                if (news.Count() > SIMILAR_NEWS_WINDOW)
                {
                    news.RemoveAt(0);
                }
            }

            double[] FillWeights(List<ArticleWordWeight> src)
            {
                var dest = weightsTemplate.ToDictionary(_ => _.Key, __ => (double)0);

                foreach (var w in src)
                {
                    if (dest.ContainsKey(w.Word))
                    {
                        dest[w.Word] = w.Weight;
                    }
                }

                return dest.Values.ToArray();
            }
        }

        public async Task FillNewsTags()
        {
            var weights = (await WwCollection.Find(_ => true).ToListAsync()).ToDictionary(_ => _.Word, __ => __.Weight);

            foreach (var n in await NewsCollection.Find(_ => _.Tags == null && _.WW != null).ToListAsync())
            {
                var articleWeights = n.WW
                    .Select(_ => new ArticleWordWeight
                    {
                        Weight = _.Weight * GetWordWeight(weights, _.Word),
                        Word = _.Word,
                        POS = _.POS,
                        AddTps = _.AddTps
                    }).Where(_ => _.POS == "S" && _.Word.Length > 2)
                    .OrderByDescending(_ => _.Weight);
                var simpleNouns = articleWeights.Where(_ => _.AddTps == "None").Take(5);
                var famnames = articleWeights.Where(_ => _.AddTps == "FAMN").Take(5);
                var geos = articleWeights.Where(_ => _.AddTps == "GEO").Take(5);
                var tags = new List<string>(simpleNouns.Concat(famnames).Concat(geos).Select(_ => _.Word).MakeUnique());

                tags.Shuffle();

                var filter = Builders<NewsDbModel>.Filter.Eq(_ => _.Guid, n.Guid);
                var update = Builders<NewsDbModel>.Update.Set(_ => _.Tags, tags);
                await NewsCollection.UpdateOneAsync(filter, update);

                logger.Info($"Added tags for {n.Guid}");
            }
        }

        private double GetWordWeight(Dictionary<string, double> weights, string word)
        {
            if (weights.ContainsKey(word))
            {
                return weights[word];
            }
            else
            {
                return 1.0;
            }
        }

        public async Task UpdateNewsArticlesWordsWeights()
        {
            var docs = await NewsCollection.Find(_ => _.WW == null).SortByDescending(_ => _.Date).ToListAsync();

            foreach (var n in docs)
            {
                logger.Info($"Setting words weights for {n.Guid}");

                var words = await GetArticleWords(n.Header + " " + n.Text);
                var ww = new List<ArticleWordWeight>();

                foreach (var g in words.OrderBy(_ => _.Word).GroupBy(_ => _.Word))
                {
                    var w = g.First();
                    w.Weight = g.Count() / (double)words.Length;
                    ww.Add(w);
                }

                var filter = Builders<NewsDbModel>.Filter.Eq(_ => _.Guid, n.Guid);
                var update = Builders<NewsDbModel>.Update.Set(_ => _.WW, ww);
                await NewsCollection.UpdateOneAsync(filter, update);
            }
        }

        public async Task UpdateWordsWeightsDictionary()
        {
            logger.Info("Updating global words weights dictionary.");

            await newsDatabase.DropCollectionAsync(NEWS_WORD_WEIGHTS_COLLECTION_NAME);

            Dictionary<string, double> wordWeights = new Dictionary<string, double>();

            var docs = await NewsCollection.Find(_ => _.WW != null).SortByDescending(_ => _.Date).ToListAsync();

            foreach (var n in docs)
            {
                FillWordWeightsDictionary(wordWeights, n.WW.Select(_ => _.Word).ToArray());
            }

            foreach (var k in wordWeights.Keys.ToList())
            {
                wordWeights[k] = Math.Log10((double)docs.Count / wordWeights[k]);
            }

            var resultDict = wordWeights.OrderBy(_ => _.Value).Select(_ => new WordWeightDbModel { Word = _.Key, Weight = _.Value }).ToList();

            await WwCollection.InsertManyAsync(resultDict);

            logger.Info($"Number of: words {resultDict.Count}, docs {docs.Count}");
        }

        private void FillWordWeightsDictionary(Dictionary<string, double> ww, string[] words)
        {
            foreach (var w in words)
            {
                if (ww.ContainsKey(w))
                {
                    ww[w] = ww[w] + 1.0;
                }
                else
                {
                    ww[w] = 1.0;
                }
            }
        }

        private static async Task<ArticleWordWeight[]> GetArticleWords(string text)
        {
            var words = text
                .ToLowerInvariant().SaveOnlyLetters()
                .RemoveLatinLetters().TrimAndCompactWhitespaces()
                .SplitNoEmpty(new[] { ' ' }).ToList();

            var stemmer = new MyStem();

            var stemmedWords = await stemmer.Stem(words);

            return stemmedWords
                .Where(_ => _.Lexemes.Count > 0)
                .Select(_ =>
                {
                    var lex = _.Lexemes.First();

                    return new ArticleWordWeight
                    {
                        Word = lex.BaseForm,
                        POS = lex.Grammar.POS.ToString(),
                        AddTps = lex.Grammar.AddTps.ToString()
                    };
                })
                .ToArray();
        }

        public async Task<bool> IsNewsExistsByUrl(string url)
        {
            return (await NewsCollection.Find(_ => _.Url == url).Limit(1).ToListAsync()).Count > 0;
        }

        public async Task<List<NewsDbModel>> GetLatestNews(int count = 20)
        {
            return await NewsCollection.Find(_ => true).SortByDescending(_ => _.Date).Limit(count).ToListAsync();
        }

        public async Task AddNews(NewsArticle news)
        {
            await NewsCollection.InsertOneAsync(new NewsDbModel
            {
                Guid = Guid.NewGuid().ToString(),
                Header = news.Header,
                Text = news.Text,
                Date = news.Date,
                Url = news.Url
            });
        }

        public static NewsDatabase Init(DatabaseFactory databaseFactory)
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new NewsDatabase(databaseFactory);
                    }
                }
            }

            return _instance;
        }
    }
}
