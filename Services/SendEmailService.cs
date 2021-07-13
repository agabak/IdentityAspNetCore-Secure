using IdentityAspNetCore.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Services
{
    public class SendEmailService : ISendEmailService
    {
        private readonly IEmailSender _emailSender;
        public SendEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public async Task SendEmailAsync(SendEmailViewModel model)
        {
            await _emailSender.SendEmailAsync(model.Email, model.Subject,
               model.Message);
        }
    }
}
