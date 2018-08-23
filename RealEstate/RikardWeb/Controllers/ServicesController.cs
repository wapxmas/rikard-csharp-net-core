using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult ServicesList() => View();
    }
}
