using Microsoft.AspNetCore.Mvc;
using RikardWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Components
{
    public class PagesNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string controller, string action, int prevPage, int currentPage, int nextPage, int totalPages)
        {
            return View(new PagesNavModel
            {
                Controller = controller,
                Action = action,
                PrevPage = prevPage,
                CurrentPage = currentPage,
                NextPage = nextPage,
                TotalPages = totalPages
            });
        }
    }
}
