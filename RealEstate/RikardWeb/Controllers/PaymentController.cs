using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RikardLib.AspLog;
using RikardLib.Text;
using RikardWeb.Lib.Identity;
using RikardWeb.Options;
using SmsRu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RikardWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IAspLogger logger;
        private readonly IUsersService<IdentityUser> usersService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IOptions<InfoOptions> infoOptions;
        private readonly ISmsRuService smsService;

        public PaymentController(
            IAspLogger logger, 
            UserManager<IdentityUser> userManager, 
            IOptions<InfoOptions> infoOptions,
            ISmsRuService smsService,
            IUsersService<IdentityUser> usersService)
        {
            this.logger = logger;
            this.usersService = usersService;
            this.userManager = userManager;
            this.infoOptions = infoOptions;
            this.smsService = smsService;
        }

        [HttpGet]
        public IActionResult SuccessUrl()
        {
            return RedirectToAction(nameof(ProfileController.Index), "Profile", new { SuccessfulPay = true });
        }

        [HttpGet]
        public IActionResult ResultUrl(string IncSum, string OutSum, string InvId, string SignatureValue, string Shp_id, string Shp_type)
        {
            if(!string.IsNullOrWhiteSpace(SignatureValue))
            {
                var sMrchPass2 = infoOptions.Value.Robokassa.Pass2;

                string sCrc = TextUtilites.MD5Hash($"{OutSum}:{InvId}:{sMrchPass2}:Shp_id={Shp_id}:Shp_type={Shp_type}");

                if(sCrc.ToLower() == SignatureValue.ToLower())
                {
                    double dOutSum;

                    if(double.TryParse(OutSum, NumberStyles.Number, CultureInfo.InvariantCulture, out dOutSum))
                    {
                        double dIncSum;
                        var sIncSum = IncSum;
                        if (double.TryParse(IncSum, NumberStyles.Number, CultureInfo.InvariantCulture, out dIncSum))
                        {
                            sIncSum = dIncSum.ToString("0.00", CultureInfo.InvariantCulture);
                        }
                        usersService.AddProlongationPayment(Shp_id, dOutSum, Shp_type, $"Robokassa: сумма с комиссией {sIncSum}");
                        smsService.SendSms(infoOptions.Value.SmsNotifyPhone, $"Payment: {dOutSum.ToString("0.00", CultureInfo.InvariantCulture)}");
                    }
                    else
                    {
                        logger.Error($"Invalid OutSum on payment: {OutSum}");
                    }
                }
                else
                {
                    logger.Error($"Invalid payment: [{OutSum}:{InvId}:[PASS2]:Shp_id={Shp_id}:Shp_type={Shp_type}] <> {SignatureValue}");
                }
            }

            return Content($"OK{InvId}");
        }

        [HttpGet]
        public IActionResult FailUrl()
        {
            return RedirectToAction(nameof(ProfileController.Index), "Profile");
        }
    }
}
