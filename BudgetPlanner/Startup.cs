using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BudgetPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Authentication.Cookies;
using BudgetPlanner.DataAccess.Data;
using BudgetPlanner.Services;
using Serilog;
using Serilog.Events;

namespace BudgetPlanner
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            //Log.Logger = new LoggerConfiguration()
               // .MinimumLevel.Debug() // Minimum log level.
               // .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // Log information from the Microsoft namespace has a minimum of Information.
               // .Enrich.FromLogContext() // Add additional data to the log.
              //  .WriteTo.Console(outputTemplate: "{Timestamp} [{Level:u3} {RequestId}] {Message} {Exception}{NewLine}") // Write log information to the console.
               // .WriteTo.File("logs/MarysToyStore.txt", outputTemplate: "{Timestamp} [{Level:u3} {RequestId}] {Message} {Exception}{NewLine}") // Create a log file.
              //  .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfig>(Configuration.GetSection("AppConfig"));
            services.AddControllersWithViews();

            services.AddDbContext<BudgetDataContext>
                (options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.LoginPath = "/sign-in";
                    options.LogoutPath = "/sign-out";
                    options.AccessDeniedPath = "/access-denied";
                    options.Cookie.Name = "UserAuth";
                    options.ExpireTimeSpan = TimeSpan.FromDays(2);
                    options.SlidingExpiration = true;
                });

            // Add USPS Service to DI.
            services.AddSingleton<UspsService>(new UspsService(
            Configuration.GetValue<string>("UspsAddressVerificationUrl"), 
            Configuration.GetValue<string>("UspsToken")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
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
