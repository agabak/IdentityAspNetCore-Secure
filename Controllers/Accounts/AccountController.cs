using IdentityAspNetCore.Models;
using IdentityAspNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Controllers.Accounts
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ISendEmailService _sendEmail;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        public AccountController(
            IAccountService accountService,
            ISendEmailService sendEmail,
           SignInManager<AppUser> signInManager,
           UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _sendEmail = sendEmail;
            _signInManager = signInManager;
            _userManager = userManager;
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
                var userIdCode = await _accountService.EmailConfirmationCode(model.Email);

                await SendEmailConfirmationEmail(model.Email, userIdCode);

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

            await SendResetPasswordEmail(model.Email, user);
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
            if (user.Succeeded) return View("ResetPasswordConfirmation");
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

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code)) 
                 return View("Error");
            var result = await _accountService.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(
            string provider, string returnurl = null)
        {
            var redirecturl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUlr = returnurl });
            var properties = _accountService
                           .ConfigureExternalAuthenticationProperties(provider, redirecturl);
            return Challenge(properties,provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(
            string returnurl, string remoteError = null)
        {
            if(remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));

            var result = await _signInManager
                .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,false);
            if(result.Succeeded)
            { 
                // update any authentication tokens
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return LocalRedirect(returnurl);
            }
            else
            {
                //If the user does not have account, then will ask the user to create an account.
                ViewData["ReturnUrl"] = returnurl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName; 

                // value you getting from the external provider..
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ExternalLoginConfirmation", 
               new ExternalLoginConfirmationViewModel 
               { Email = email, FirstName = name, LastName = name });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation
            (ExternalLoginConfirmationViewModel model, string returnurl)
        {
            if(ModelState.IsValid)
            {
                // get the information
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info is null) return View("Error");

                var user = new AppUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBorn = DateTime.Now,
                    LastDateLogin = DateTime.Now,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user);
                if(result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return LocalRedirect(returnurl);
                    }
                }
                AddErrors(result);
            }
            ViewData["ReturnUrl"] = returnurl;
            return View(model);
        }

        private async Task SendResetPasswordEmail(string email, (string, string) user)
        {
            var callbackurl = Url.Action("ResetPassword", "Account",
                new { userId = user.Item1, code = user.Item2 }, protocol: HttpContext.Request.Scheme);

            SendEmailViewModel sendEmailModel = new()
            {
                Email = email,
                Subject = "Reset Password",
                Message = "Please reset your password by clicking here: <a href=\"" + callbackurl + "\"> link"
            };
            await _sendEmail.SendEmailAsync(sendEmailModel);
        }

        private async Task SendEmailConfirmationEmail(string email, (string, string) userIdCode)
        {
            var callbackurl = Url.Action("ConfirmEmail", "Account",
                 new { userId = userIdCode.Item1, code = userIdCode.Item2 }, protocol: HttpContext.Request.Scheme);

            SendEmailViewModel sendEmailModel = new()
            {
                Email = email,
                Subject = "Confirm your account",
                Message = "Please reset your password by clicking here: <a href=\"" + callbackurl + "\"> link"
            };
            await _sendEmail.SendEmailAsync(sendEmailModel);
        }
    }
}
