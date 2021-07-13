using IdentityAspNetCore.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> RegisterUser(RegisterViewModel model)
        {
            if (model is null) return null;
            AppUser user = new()
            {
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBorn = DateTime.Now,
                LastDateLogin = DateTime.Now,
                Email = model.Email,
            };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<SignInResult> LoginUserAsync(
                 RegisterViewModel registerView = null,
                 LoginViewModel loginView = null)
        {
            string userName = string.Empty;
            string password = string.Empty;
            bool rememberMe = false;
            if (loginView is null && registerView is null) return null;
            if (loginView is not null)
            {
                userName = loginView.Email;
                password = loginView.Password;
                rememberMe = loginView.RememberMe;
                
            }
            else
            {
                userName = registerView.Email;
                password = registerView.Password;
            }
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userManager.FindByEmailAsync(userName);
            if (user is null) return null;
            return await _signInManager
                  .PasswordSignInAsync(user, password, loginView.RememberMe, true);
        }

        public async Task<(string,string)> ForgotPasswordCode(ForgotPasswordViewModel model)
        {
            if (model is null) return (null,null);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return (null, null);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            return (user.Id, code);
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (model is null) return null;
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return null;
            return await _userManager
                .ResetPasswordAsync(user, model.Code, model.Password);
        }
    }
}
