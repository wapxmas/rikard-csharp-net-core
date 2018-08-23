using Microsoft.AspNetCore.Mvc;
using RikardWeb.Lib.News.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Services;
using RikardWeb.Lib.News.Services;

namespace RikardWeb.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsDatabaseService newsService;

        public NewsController(INewsDatabaseService newsService)
        {
            this.newsService = newsService;
        }

        public async Task<IActionResult> NewsArticle(Guid article)
        {
            var n = await newsService.GetNewsArticleByGuid(article.ToString());

            if (n == null)
            {
                return NotFound();
            }
            else
            {
                return View(n);
            }
        }

        public IActionResult NewsList(int page)
        {
            return View(page);
        }
    }
}
