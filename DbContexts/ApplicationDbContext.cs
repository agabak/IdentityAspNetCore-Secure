using IdentityAspNetCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityAspNetCore.DbContexts
{
    public class ApplicationDbContext: IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) :base(options)
        { }
    }
}
