using System;
using System.Collections.Generic;
using System.Text;

namespace RikardWeb.Lib.News.Data
{
    public class ArticleWordWeight
    {
        public string Word { get; set; }
        public double Weight { get; set; }
        public string POS { get; set; }
        public string AddTps { get; set; }
    }
}
