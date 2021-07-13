using IdentityAspNetCore.Configuration;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace IdentityAspNetCore.Services
{
    // need negut package Microsoft.AspNetCore.Identity.UI 
    // for IEmailSender
    public class MailJetEmailSenderService : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly MailJetOptions _mailJetOptions;
        public MailJetEmailSenderService(IConfiguration configuration)
        {
            _config = configuration;
            _mailJetOptions = _config.GetSection("MailJetOptions").Get<MailJetOptions>();
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailjetClient client = new MailjetClient(_mailJetOptions.ApiKey,_mailJetOptions.SecretKey)
            {
                // Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }.Property(Send.Messages, new JArray {
               new JObject {
                      {
                       "From",
                       new JObject {
                        {"Email", "agaba_k@protonmail.com"},
                        {"Name", "Agaba"}
                       }
                      }, {
                       "To",
                       new JArray {
                        new JObject {
                         {
                          "Email",
                          email
                         }, {
                          "Name",
                          "Agaba"
                         }
                        }
                       }
                      }, {
                       "Subject",
                       subject
                      }, {
                       "HTMLPart",
                       htmlMessage
                      }
               }
             });
            MailjetResponse response = await client.PostAsync(request);
            if(response.IsSuccessStatusCode)
            {
              // do some loggiing
            }else
            {
                // more logging
            }
        }
    }
}

