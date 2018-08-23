using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Models
{
    public class PagesNavModel
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public int PrevPage { get; set; }
        public int CurrentPage { get; set; }
        public int NextPage { get; set; }
        public int TotalPages { get; set; }
    }
}
