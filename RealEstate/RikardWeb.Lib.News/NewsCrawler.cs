using AngleSharp.Parser.Html;
using RikardLib.Log;
using RikardLib.Text;
using RikardLib.Web;
using RikardWeb.Lib.News.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RikardWeb.Lib.News
{
    public class NewsCrawler
    {
        private readonly Logger logger = new Logger();

        private readonly string[] ExceedFragments = { "всегда на связи!", "всегда на связи", "отдел по связям с общественностью и сми" };
        private readonly string[] StopWords = { "Документация", "Конкурсная документация" };

        public async Task<List<string>> CrawlNewsListAsync(string Url)
        {
            if (Url == null)
            {
                throw new ArgumentNullException("url argument of the NewsParser constructor cannot be null");
            }

            var newsList = new List<string>();

            logger.Info($"Downloading {Url}");

            try
            {
                HttpRequestResult result = await HttpRequest.
                    HttpGetRequest(Url,
                    "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0",
                    allowRedirect: true);

                logger.Info($"Have downloaded {Url}");

                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    var parser = new HtmlParser();
                    var document = await parser.ParseAsync(result.Data);
                    foreach (var e in document.QuerySelectorAll("ul.newslist li p strong a"))
                    {
                        if (e.HasAttribute("href"))
                        {
                            newsList.Add(CompoundUrl(Url, e.GetAttribute("href")));
                        }
                    }
                }
                else
                {
                    logger.Warn($"Incorrect HTTP code: {result.HttpStatusCode}, {Url}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"While downloading {Url}", e);
            }

            return newsList;
        }

        public async Task<NewsArticle> DownloadSingleNewsAsync(string articleUrl)
        {
            logger.Info($"Downloading article {articleUrl}");

            NewsArticle na = null;

            try
            {
                HttpRequestResult result = await HttpRequest.
                    HttpGetRequest(articleUrl,
                    "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0",
                    allowRedirect: true);

                logger.Info($"Have downloaded article {articleUrl}");

                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    var parser = new HtmlParser();
                    var document = await parser.ParseAsync(result.Data);

                    var articleHeader = document.QuerySelector("div.primary_content h1");
                    var articleDate = document.QuerySelector("div.primary_content div.b-text-content div.news_date");
                    var articleHtml = document.QuerySelector("div.primary_content div article");

                    if (articleHeader != null && articleDate != null && articleHtml != null)
                    {
                        var text = new List<string>();

                        foreach (var p in articleHtml.TextContent.SplitNoEmptyTrim(new char[] { '\n', '\r' }))
                        {
                            var str = p.TrimAndCompactWhitespaces();

                            if (ExceedFragments.Any(_ => str.ToLowerInvariant() == _.ToLowerInvariant()))
                            {
                                break;
                            }

                            if (StopWords.Any(_ => str.ToLowerInvariant() == _.ToLowerInvariant()))
                            {
                                continue;
                            }

                            if (str.Length > 0)
                            {
                                text.Add(str);
                            }
                        }

                        var pic = articleHtml.QuerySelector("img")?.GetAttribute("src");

                        if (pic != null)
                        {
                            pic = WebUtility.UrlDecode(pic);
                            pic = CompoundUrl(new Uri(articleUrl), Uri.EscapeUriString(pic));
                            pic = await HttpUtilites.IsUrlExists(pic) ? pic : null;
                        }

                        if (text.Count == 0)
                        {
                            logger.Warn($"News article doesn't have content, {articleUrl}");
                            return na;
                        }

                        var headerText = articleHeader.TextContent.TrimAndCompactWhitespaces();

                        if(String.IsNullOrWhiteSpace(headerText))
                        {
                            logger.Warn($"News article doesn't have header, {articleUrl}");
                            return na;
                        }

                        DateTime date;

                        try
                        {
                            date = DateTime.ParseExact(
                                RetrieveDayDate(articleDate.TextContent),
                                "dd.MM.yyyy",
                                CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            date = DateTime.Now;
                        }

                        na = new NewsArticle
                        {
                            Header = headerText,
                            Text = string.Join("\n", text),
                            Url = articleUrl,
                            Date = date,
                            PictureUrl = pic
                        };
                    }
                    else
                    {
                        logger.Warn($"News article doesn't contain neccessary item, {articleUrl}");
                    }
                }
                else
                {
                    logger.Warn($"Incorrect HTTP code appeared while downloading article: {result.HttpStatusCode}, {articleUrl}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"While downloading article {articleUrl}", e);
            }

            return na;
        }

        private string RetrieveDayDate(string date)
        {
            var dateText = date.TrimAndCompactWhitespaces();
            var parts = dateText.SplitDefault();

            if (parts.Count() == 1)
            {
                return parts.First();
            }

            foreach(var p in parts)
            {
                if(p.Contains("."))
                {
                    return p;
                }
            }

            return dateText;
        }

        private string CompoundUrl(string baseUrl, string pageUrl)
        {
            if (pageUrl.StartsWith("https://") || pageUrl.StartsWith("http://"))
            {
                return pageUrl;
            }

            Uri baseUri = new Uri(baseUrl);

            pageUrl = pageUrl.StartsWith("/") ? pageUrl : "/" + pageUrl;

            return baseUri.Scheme + "://" + baseUri.Host + pageUrl;
        }

        private string CompoundUrl(Uri baseUri, string pageUrl)
        {
            if (pageUrl.StartsWith("https://") || pageUrl.StartsWith("http://"))
            {
                return pageUrl;
            }

            pageUrl = pageUrl.StartsWith("/") ? pageUrl : "/" + pageUrl;

            return baseUri.Scheme + "://" + baseUri.Host + pageUrl;
        }
    }
}

