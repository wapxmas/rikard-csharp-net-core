using RikardWeb.Lib.Db;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RikardLib.Text;
using Tweetinvi;
using Tweetinvi.Models;
using RikardLib.Web;
using System.IO;
using RikardLib.Log;
using System.Threading;

namespace RikardWeb.Lib.News
{
    public class NewsTweeter
    {
        private const int TWITTER_MAX_LENGHT = 117;
        private const string NEWS_URL_PREFIX = "https://rikard.ru/News/NewsArticle/";

        private readonly Logger logger = new Logger();

        private readonly NewsDatabase newsDatabase;
        private Random random;
        private readonly NewsCrawler newsCrawler;

        private NewsTweeter()
        {
            this.newsDatabase = NewsDatabase.Instance;
            this.random = new Random();
            this.newsCrawler = new NewsCrawler();
        }

        public static async Task RunNewsTweeter()
        {
            var newsUpdater = new NewsTweeter();
            await newsUpdater.Tweet();
        }

        public static async Task RunNewsTweeter(MongoDbCredentials credentials)
        {
            DatabaseFactory databaseFactory = DatabaseFactory.Init(credentials);
            NewsDatabase newsDatabase = NewsDatabase.Init(databaseFactory);

            await RunNewsTweeter();
        }

        private async Task Tweet()
        {
            var lastTweet = await newsDatabase.GetLastTweet();

            bool canDoNewTweet = lastTweet == null ? true : DateTime.Now.Subtract(lastTweet.Tweet.Date).Minutes > GetRandomMinutes();

            if (canDoNewTweet)
            {
                var nextNewsNoTwitter = (await newsDatabase.GetNewsNoTweet(1)).FirstOrDefault();

                if (nextNewsNoTwitter != null)
                {
                    var t = nextNewsNoTwitter.Tags == null ? String.Empty : 
                        TrimWords(nextNewsNoTwitter.Tags.Take(5).Select(_ => "#" + _), TWITTER_MAX_LENGHT - 1 
                        /* subtract 1 `space` char between link and tags */);
                    var h = TrimWords(nextNewsNoTwitter.Header.SaveLettersAndNumbers().
                        TrimAndCompactWhitespaces().SplitNoEmpty(new char[] { ' ' }), 
                        TWITTER_MAX_LENGHT - t.Length - 2 
                        /* subtract 1 `space` char between header and link, and 1 `space` char between link and tags */);

                    var newsUrl = NEWS_URL_PREFIX + nextNewsNoTwitter.Guid;

                    var twitterMsg = new string[] { h, newsUrl, t };

                    Auth.SetUserCredentials(
                        "KaKE385JQWliKR3bzpUpw",
                        "LKyCUv4VsYb4e6aCsTCwn2ygwnEjrCj7D3fgQeDFQk",
                        "624687706-2Y87CmUZDsxs9dsIDcDduCexHOXy51xPrZTaezZL",
                        "wMqexZoDcvpzyj7vorf9Sd94QFkOorYnRugY7qRxvk");

                    var twitter = User.GetAuthenticatedUser();

                    if (twitter != null)
                    {
                        byte[] img = await GetNewsImage(nextNewsNoTwitter.Url);

                        ITweet tweet = null;

                        string tweetText = String.Join(" ", twitterMsg);

                        if (img == null)
                        {
                            tweet = await TweetAsync.PublishTweet(tweetText);
                        }
                        else
                        {
                            tweet = await TweetAsync.PublishTweetWithImage(tweetText, img);
                        }

                        if (tweet != null)
                        {
                            await newsDatabase.UpdateNewsTweet(nextNewsNoTwitter.Guid, tweet.IdStr, DateTime.Now);
                            logger.Info($"New tweet: {tweet.IdStr}/{nextNewsNoTwitter.Guid} {tweetText}");
                        }
                        else
                        {
                            var ex = Tweetinvi.ExceptionHandler.GetLastException();

                            if(!String.IsNullOrWhiteSpace(ex?.TwitterDescription))
                            {
                                if(ex.StatusCode == 400)
                                {
                                    await newsDatabase.UpdateNewsTweet(nextNewsNoTwitter.Guid, ex.StatusCode.ToString(), DateTime.Now);
                                }
                                else
                                {
                                    logger.Warn($"{ex.StatusCode} | {ex.TwitterDescription} | {tweetText} | {h.Length + t.Length + 2}");
                                }
                            }
                        }

                        await Task.Delay(1000);
                    }
                }
            }
        }

        private async Task<byte[]> GetNewsImage(string url)
        {
            if(String.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var n = await newsCrawler.DownloadSingleNewsAsync(url);

            if(n == null || String.IsNullOrWhiteSpace(n.PictureUrl))
            {
                return null;
            }

            try
            {
                var img = await HttpUtilites.GetBytesByUrl(n.PictureUrl, 60*5);
                
                if (img == null || img.Length == 0 || img.Length > 1024*1024*5)
                {
                    return null;
                }

                return img;
            }
            catch
            {
                return null;
            }
        }

        private string TrimWords(IEnumerable<string> words, int size)
        {
            var _words = new List<string>();

            foreach(var t in words)
            {
                size -= (t.Length + 1);

                _words.Add(t);

                if (size <= 0)
                {
                    break;
                }
            }

            if(size < -1)
            {
                _words.RemoveAt(_words.Count - 1);
            }

            return String.Join(" ", _words);
        }

        private int GetRandomMinutes()
        {
            return random.Next(5, 40);
        }
    }
}
