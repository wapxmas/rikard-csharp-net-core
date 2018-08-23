using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RikardLib.AspHelpers.Services;
using RikardLib.AspLog;
using RikardWeb.Data;
using RikardWeb.Lib.Identity;
using RikardWeb.Models;
using RikardWeb.Options;
using RikardWeb.Services;
using SmsRu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IAspLogger logger;
        private readonly ISendEmailService sendEmail;
        private readonly IViewRenderService viewRender;
        private readonly ISmsRuService smsService;
        private readonly IUsersService<IdentityUser> usersService;
        private readonly IOptions<InfoOptions> infoOptions;
        private readonly IHostingEnvironment env;

        public ProfileController(
            IAspLogger logger, 
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager,
            ISendEmailService sendEmail,
            ISmsRuService smsService,
            IUsersService<IdentityUser> usersService,
            IOptions<InfoOptions> infoOptions,
            IHostingEnvironment env,
            IViewRenderService viewRender)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.sendEmail = sendEmail;
            this.smsService = smsService;
            this.viewRender = viewRender;
            this.usersService = usersService;
            this.infoOptions = infoOptions;
            this.env = env;
        }

        public async Task<IActionResult> Index(bool SuccessfulPay)
        {
            ViewBag.SuccessfulPay = SuccessfulPay;
            return View(await userManager.GetUserAsync(User));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel customer, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(customer.Email, customer.Password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    logger.Info($"{customer.Email} is logged in");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    var user = await userManager.FindByEmailAsync(customer.Email);
                    string error = null;
                    if(user != null)
                    {
                        if(!user.EmailConfirmed)
                        {
                            error = "Вам необходимо подтвердить Email указанный при регистрации, проверьте почту.";
                        }
                        else if(!user.PhoneNumberConfirmed)
                        {
                            return RedirectToAction("ConfirmPhone", new { userId = user.Id });
                        }
                    }
                    ModelState.AddModelError("", error ?? "Неверный Email или пароль.");
                }
            }

            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhone(string userId)
        {
            var user = userId == null ? null : await userManager.FindByIdAsync(userId);

            if(user != null && user.PhoneNumberConfirmed)
            {
                user = null;
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }
            else
            {
                return View(new ConfirmPhoneModel { Id = user.Id, Phone = user.PhoneNumber });
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhone(ConfirmPhoneModel confirm)
        {
            var user = confirm.Id == null ? null : await userManager.FindByIdAsync(confirm.Id);

            if (user != null && user.PhoneNumberConfirmed)
            {
                user = null;
            }

            if(user != null && ModelState.IsValid)
            {
                bool phoneError = false;

                if(user.PhoneNumber != confirm.Phone)
                {
                    if(!await usersService.IsPhoneNumberExists(confirm.Phone, user.Id))
                    {
                        await userManager.SetPhoneNumberAsync(user, confirm.Phone);
                    }
                    else
                    {
                        phoneError = true;

                        ModelState.AddModelError("", "Такой телефон уже зарегистрирован, укажите другой.");
                    }
                }

                if(!phoneError)
                {
                    smsService.SendSms(confirm.Phone, await userManager.GenerateChangePhoneNumberTokenAsync(user, confirm.Phone));
                    return RedirectToAction("ConfirmPhoneCheck", new { userId = user.Id });
                }
            }

            if(user == null)
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhoneCheck(string userId)
        {
            var user = userId == null ? null : await userManager.FindByIdAsync(userId);

            if (user != null && user.PhoneNumberConfirmed)
            {
                user = null;
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }
            else
            {
                return View(new ConfirmPhoneCheckModel { Id = user.Id });
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmPhoneCheck(ConfirmPhoneCheckModel confirm)
        {
            var user = confirm?.Id == null ? null : await userManager.FindByIdAsync(confirm.Id);

            if (user != null && user.PhoneNumberConfirmed)
            {
                user = null;
            }

            if (user != null && ModelState.IsValid)
            {
                if(await userManager.VerifyChangePhoneNumberTokenAsync(user, confirm.Code, user.PhoneNumber))
                {
                    user.PhoneNumberConfirmed = true;
                    await userManager.UpdateAsync(user);
                    await signInManager.SignInAsync(user, true);
                    smsService.SendSms(infoOptions.Value.SmsNotifyPhone, $"New customer: {user.Email}");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Введен неверный код");
                }
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel reset)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(reset.Email);

                if(user != null)
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = MakeAction(nameof(ResetPasswordCode), "Profile", new { userId = user.Id, code = code });
                    string emailBody = await viewRender.RenderToStringAsync("Templates/ResetPasswordLetterBody", callbackUrl);
                    sendEmail.Send(new EmailLetter { To = reset.Email, Subject = "Восстановление пароля", Body = emailBody });
                }

                return RedirectToAction(nameof(ResetPasswordSendOk));
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordCode(string userId, string code)
        {
            if(!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(code))
            {
                return View(new ResetPasswordCodeModel() { Id = userId, Code = code });
            }
            else
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordCode(ResetPasswordCodeModel reset)
        {
            var user = reset?.Id == null ? null : await userManager.FindByIdAsync(reset.Id);

            if(user != null)
            {
                if(ModelState.IsValid)
                {
                    var result = await userManager.ResetPasswordAsync(user, reset.Code, reset.Password);

                    if (result.Succeeded)
                    {
                        await signInManager.SignInAsync(user, true);
                        return RedirectToAction(nameof(Index));
                    }

                    AddErrors(result.Errors);
                }
            }
            else
            {
                ModelState.AddModelError("", "Ошибочный запрос");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordSendOk()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return Redirect("/");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel newCustomer)
        {
            if (ModelState.IsValid)
            {
                var customer = new IdentityUser { PhoneNumber = newCustomer.Phone, Email = newCustomer.Email, Name = newCustomer.Email };
                var user = await userManager.FindByEmailAsync(newCustomer.Email);
                if (user == null)
                {
                    bool phoneExists = await usersService.IsPhoneNumberExists(newCustomer.Phone);

                    if(!phoneExists)
                    {
                        customer.AddToRole("Customer");

                        var result = await userManager.CreateAsync(customer, newCustomer.Password);

                        if (result.Succeeded)
                        {
                            var code = await userManager.GenerateEmailConfirmationTokenAsync(customer);
                            var callbackUrl = MakeAction(nameof(ConfirmEmail), "Profile", new { userId = customer.Id, code = code });
                            string emailBody = await viewRender.RenderToStringAsync("Templates/ActivationLetterBody", callbackUrl);
                            sendEmail.Send(new EmailLetter { To = customer.Email, Subject = "Активация регистрации", Body = emailBody });
                            return RedirectToAction(nameof(RegisterOK));
                        }
                        else
                        {
                            AddErrors(result.Errors);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Пользователь с таким телефоном уже зарегистрирован.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь с таким e-mail уже зарегистрирован.");
                }
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("ConfirmEmail", "Incorrect");
            }

            var user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                return View("ConfirmEmail", "NoUser");
            }

            var result = await userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View("ConfirmEmail", "Confirm");
            }

            AddErrors(result.Errors);

            return View();
        }

        [NonAction]
        private void AddErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var e in errors)
            {
                ModelState.AddModelError("", e.Description);
            }
        }

        [AllowAnonymous]
        public IActionResult RegisterOK()
        {
            return View();
        }

        private string MakeAction(string action, string controller, object values)
        {
            if(env.IsDevelopment())
            {
                return Url.Action(action, controller, values, protocol: Request.Scheme);
            }
            else
            {
                return Url.Action(action, controller, values, protocol: infoOptions.Value.Host.Scheme, host: infoOptions.Value.Host.Name);
            }
        }
    }
}
