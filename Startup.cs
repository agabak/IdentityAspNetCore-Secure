using IdentityAspNetCore.DbContexts;
using IdentityAspNetCore.Models;
using IdentityAspNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IdentityAspNetCore
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(
                opts => opts.UseSqlServer(_config.GetConnectionString("DefaultConnection")));
        
            services.AddIdentity<AppUser, IdentityRole>(opt => 
            {
                opt.Password.RequiredLength = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
                opt.Lockout.MaxFailedAccessAttempts = 3;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            // add external login add here
            // Microsoft.AspNetCore.Authentication.OpenIdConnect
            services.AddAuthentication()
                    .AddOpenIdConnect("AzureAd", "Login with Azure Ad",
                    opts => _config.Bind("AzureAd", opts));

            // register email sender 
            services.AddTransient<IEmailSender, MailJetEmailSenderService>();
            services.AddTransient<ISendEmailService, SendEmailService>();
            //configure identity option for you're needs
           //  services.Configure<IdentityOptions>(opt =>{});

            services.AddScoped<IAccountService, AccountService>();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
