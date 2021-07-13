using Microsoft.AspNetCore.Identity;
using System;

namespace IdentityAspNetCore.Models
{
    public class AppUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBorn { get; set; }
        public DateTime? LastDateLogin { get; set; }
    }
}
