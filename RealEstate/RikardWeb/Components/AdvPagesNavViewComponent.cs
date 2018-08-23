using Microsoft.AspNetCore.Mvc;
using RikardWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Components
{
    public class AdvPagesNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(bool isNextPageExists)
        {
            return View(new AdvPagesNavModel
            {
                IsNextPageExists = isNextPageExists
            });
        }
    }
}
