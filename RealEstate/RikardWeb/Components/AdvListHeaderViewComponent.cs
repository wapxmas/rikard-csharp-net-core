using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Components
{
    public class AdvListHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string title)
        {
            return View(title as object);
        }
    }
}
