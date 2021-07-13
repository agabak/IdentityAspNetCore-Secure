using IdentityAspNetCore.Models;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Services
{
    public interface ISendEmailService
    {
        Task SendEmailAsync(SendEmailViewModel model);
    }
}
