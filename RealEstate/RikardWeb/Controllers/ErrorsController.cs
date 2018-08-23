using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult ShowError(string errid)
        {
            return View(nameof(ShowError), errid);
        }
    }
}
