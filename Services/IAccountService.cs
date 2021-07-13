using IdentityAspNetCore.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUser(RegisterViewModel model);
        Task<(string, string)> ForgotPasswordCode(ForgotPasswordViewModel model);
        Task<SignInResult> LoginUserAsync(
                 RegisterViewModel registerView = null,
               LoginViewModel loginView = null);
        Task Logout();
        Task<IdentityResult> ResetPassword(ResetPasswordViewModel model);
        Task<(string,string)> EmailConfirmationCode(string userName);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string code);

    }
}