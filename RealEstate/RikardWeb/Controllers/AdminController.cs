using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RikardLib.AspLog;
using RikardWeb.Lib.Adverts.Service;
using RikardWeb.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAspLogger logger;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUsersService<IdentityUser> usersService;
        private readonly IAdvertsDatabaseService advertsDatabaseService;

        public AdminController(
            IAspLogger logger,
            IUsersService<IdentityUser> usersService,
            UserManager<IdentityUser> userManager,
            IAdvertsDatabaseService advertsDatabaseService
            )
        {
            this.logger = logger;
            this.userManager = userManager;
            this.usersService = usersService;
            this.advertsDatabaseService = advertsDatabaseService;
        }

        [HttpPost]
        public IActionResult ProlongService(string userEmail, string userId, string proType)
        {
            if(!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(proType) && !string.IsNullOrWhiteSpace(userEmail))
            {
                HttpContext.Session.SetString("Message", $"Сервис успешно продлен для {userEmail}");
                usersService.AddProlongationPayment(userId, 0.00, proType, "Сервис продлен администрацией");
            }
            else
            {
                HttpContext.Session.SetString("Message", "Нехватает данных для продления");
            }

            return RedirectToAction(nameof(AdminController.Users));
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Database()
        {
            return View();
        }

        public IActionResult DatabaseAdvertsUpdate()
        {
            advertsDatabaseService.RunAdvertsUpdater();

            HttpContext.Session.SetString("Message", "Обновление базы данных запланировано успешно");

            return RedirectToAction(nameof(AdminController.Database));
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
