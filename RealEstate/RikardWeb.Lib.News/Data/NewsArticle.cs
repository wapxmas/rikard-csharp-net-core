using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.News.Data
{
    public class NewsArticle
    {
        public string Header { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public string PictureUrl { get; set; }
    }
}
