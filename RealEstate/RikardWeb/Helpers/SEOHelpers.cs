using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Helpers
{
    public static class SEOHelpers
    {
        public static string MakeTitle(string title, string district, string area, string purpose, string floor, string location, string page)
        {
            var titles = new List<string>();

            if (!string.IsNullOrWhiteSpace(district))
            {
                titles.Add(district);
            }

            if (!string.IsNullOrWhiteSpace(purpose))
            {
                titles.Add(purpose);
            }

            if (!string.IsNullOrWhiteSpace(area))
            {
                titles.Add($"{area} кв.м.");
            }

            if (!string.IsNullOrWhiteSpace(floor))
            {
                titles.Add(floor);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                switch(location)
                {
                    case "sadovoe":
                        titles.Add("в пределах садового кольца");
                        break;
                    case "ttk":
                        titles.Add("в пределах третьего транспортного кольца");
                        break;
                    case "within-mkad":
                        titles.Add("в пределах мкад");
                        break;
                    case "outside-mkad":
                        titles.Add("за границами мкад");
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(page))
            {
                titles.Add($"страница {page}");
            }

            if(titles.Count == 0)
            {
                return title;
            }

            return $"{title} {string.Join(" ", titles)}";
        }
    }
}
