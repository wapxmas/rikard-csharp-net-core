using Iveonik.Stemmers;
using MongoDB.Driver;
using RikardLib.Log;
using RikardLib.Text;
using RikardLib.Web;
using RikardWeb.Lib.Db;
using RikardWeb.Lib.News.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikardWeb.Lib.News
{
    public class NewsUpdater
    {
        private const int MIN_NEWS_TEXT_WORDS_QUANTITY      = 10;
        private const int MIN_NEWS_HEADER_WORDS_QUANTITY    = 3;

        private readonly NewsDatabase newsDatabase;

        private readonly Logger logger = new Logger();

        private readonly IStemmer stemmer = new RussianStemmer();

        private string[] newsCatalogues =
        {
            "http://dgi.mos.ru/presscenter/news/",
            "http://tender.mos.ru/presscenter/news/",
            "http://dgi.mos.ru/presscenter/publications/",
            "http://dgi.mos.ru/presscenter/moscow-news/"
        };

        private NewsUpdater()
        {
            this.newsDatabase = NewsDatabase.Instance;
        }

        private async Task Update()
        {
            var newsUrls = new List<string>();
            var newsCrawler = new NewsCrawler();

            foreach (var cat in newsCatalogues.Select(newsCrawler.CrawlNewsListAsync).ToArray())
            {
                newsUrls.AddRange(await cat);
            }

            if (newsUrls.Count > 0)
            {
                foreach(var url in newsUrls)
                {
                    try
                    {
                        if(await HttpUtilites.IsUrlExists(url))
                        {
                            if(!await newsDatabase.IsNewsExistsByUrl(url))
                            {
                                var newsArticle = await newsCrawler.DownloadSingleNewsAsync(url);

                                if (newsArticle != null)
                                {
                                    if(SimplifyText(newsArticle.Text).SplitDefault().MakeUnique().Count() > MIN_NEWS_TEXT_WORDS_QUANTITY)
                                    {
                                        var latestNews = await newsDatabase.GetLatestNews();

                                        if(CheckHeaderWordsSimilarity(newsArticle.Header, latestNews))
                                        {
                                            logger.Info($"Adding news article: {newsArticle.Url}");

                                            await newsDatabase.AddNews(newsArticle);
                                            await newsDatabase.UpdateNewsArticlesWordsWeights();
                                            await newsDatabase.UpdateWordsWeightsDictionary();
                                            await newsDatabase.FillNewsTags();
                                            await newsDatabase.FillSimilarNews();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"News page {url} - 404 not found.");
                        }
                    }
                    catch(OperationCanceledException)
                    {
                        logger.Info($"News page {url} - timed out");
                    }
                }
            }
        }

        /**
         * Function finds similar news headers, those headers have minimum quantity (or none)
         * of unique words in comparison with new one is waiting to be added
         */

        private bool CheckHeaderWordsSimilarity(string header, List<NewsDbModel> latestNews)
        {
            var headerUnique1 = SimplifyText(header).SplitDefault().Select(_ => stemmer.Stem(_)).MakeUnique();

            int minimum = headerUnique1.Count();

            foreach (var n in latestNews)
            {
                var headerUnique2 = SimplifyText(n.Header).SplitDefault().Select(_ => stemmer.Stem(_)).MakeUnique();

                if (headerUnique2.All(_ => headerUnique1.Contains(_)))
                {
                    return false;
                }
            }

            if(minimum <= MIN_NEWS_HEADER_WORDS_QUANTITY)
            {
                return true;
            }

            foreach (var n in latestNews)
            {
                var headerUnique2 = SimplifyText(n.Header).SplitDefault().Select(_ => stemmer.Stem(_)).MakeUnique();

                minimum = Math.Min(minimum, headerUnique1.Except(headerUnique2).Count());

                if (minimum <= MIN_NEWS_HEADER_WORDS_QUANTITY)
                {
                    return false;
                }
            }

            return true;
        }

        private string SimplifyText(string header)
        {
            return header
                .ToLowerInvariant()
                .SaveOnlyLetters()
                .RemoveLatinLetters()
                .TrimAndCompactWhitespaces();
        }

        public static async Task RunNewsUpdater()
        {
            var newsUpdater = new NewsUpdater();
            await newsUpdater.Update();
        }
    }
}
