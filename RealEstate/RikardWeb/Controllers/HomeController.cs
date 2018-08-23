using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Models;
using RikardLib.AspLog;
using RikardLib.AspHelpers.Services;
using RikardWeb.Services;
using Microsoft.AspNetCore.Authorization;

namespace RikardWeb.Controllers
{
    public class HomeController : Controller
    {
        public IAspLogger Log { get; private set; }
        public IViewRenderService ViewRender { get; private set; }
        public ISendEmailService SendEmail { get; private set; }

        public HomeController(IAspLogger logger, IViewRenderService viewRender, ISendEmailService sendEmail)
        {
            Log = logger;
            ViewRender = viewRender;
            SendEmail = sendEmail;
        }

        public IActionResult Fraud()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SendLetter()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendLetter(SendLetterModel sendLetterData)
        {
            if (ModelState.IsValid)
            {
                string emailBody = await ViewRender.RenderToStringAsync("Templates/SiteLetterBody", sendLetterData);
                SendEmail.Send(sendLetterData.GenEmailLetter(emailBody));
                Log.Info("Have put letter to queue");
                return RedirectToAction(nameof(SendLetterOK));
            }
            else
            {
                return View();
            }
        }

        public IActionResult SendLetterOK()
        {
            return View();
        }

        public IActionResult Contacts() => View();

        [Authorize("Paid")]
        public IActionResult Subscribe() => View();
    }
}
