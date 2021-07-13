using IdentityAspNetCore.Models;
using IdentityAspNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Controllers.Accounts
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IEmailSender _emailSender;
        public AccountController(
            IAccountService accountService,
            IEmailSender emailSender)
        {
            _accountService = accountService;
            _emailSender = emailSender;
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnurl = null)
        {
            ViewData["ReturnUrl"] = string.IsNullOrEmpty(returnurl) ? "/" : returnurl;
            if (!ModelState.IsValid) return View(model);
            var result = await _accountService.RegisterUser(model);

            if (result.Succeeded)
            {
                var singIn = await _accountService.LoginUserAsync(model);
                if (singIn.Succeeded) return LocalRedirect(returnurl);
                return BadRequest("Fail to authenticate");
            }

            AddErrors(result);
            return View(model);
        }

        public IActionResult Login(string returnurl = null)
        {
            ViewData["ReturnUrl"] = string.IsNullOrEmpty(returnurl) ? "/" : returnurl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnurl)
        {
            ViewData["ReturnUrl"] = string.IsNullOrEmpty(returnurl) ? "/" : returnurl;
            if (ModelState.IsValid)
            {
                var result = await _accountService.LoginUserAsync(null, model);
                // make sure stay in local url
                if (result.Succeeded) return LocalRedirect(returnurl);
                if (result.IsLockedOut) return View("Lockout");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ForgotPasswordConfirmation");

            var user = await _accountService.ForgotPasswordCode(model);

            if (string.IsNullOrEmpty(user.Item1))
                return RedirectToAction("ForgotPasswordConfirmation");

            var callbackurl = Url.Action("ResetPassword", "Account",
                new { userId = user.Item1, code = user.Item2 }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                "Please reset your password by clicking here: <a href=\"" + callbackurl + "\"> link");
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation() => View();
      
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ForgotPasswordConfirmation");

           var user = await _accountService.ResetPassword(model);

            if (string.IsNullOrEmpty(user.Item1))
                return RedirectToAction("ForgotPasswordConfirmation");

            var callbackurl = Url.Action("ResetPassword", "Account",
                new { userId = user.Item1, code = user.Item2 }, protocol: HttpContext.Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                "Please reset your password by clicking here: <a href=\"" + callbackurl + "\"> link");
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ResetPasswordConfirmation() => View();

        private void AddErrors(IdentityResult result = null)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
