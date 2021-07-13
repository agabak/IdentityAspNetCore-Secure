using System.ComponentModel.DataAnnotations;

namespace IdentityAspNetCore.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
