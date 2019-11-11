using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using EmployeeManagement.Security;

namespace EmployeeManagement
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(_configuration.GetConnectionString("EmployeeDbConnection"));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(5);
            })
            .Configure<CustomEmailConfirmationTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(3);
            });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "397124056883-ophiia18d01m4fq72rf8ui55tj0l5uu0.apps.googleusercontent.com";
                    options.ClientSecret = "lTAR8-KPpk2johAkY3ed8n5o";
                })
                .AddFacebook(options =>
                {
                    options.AppId = "2439955776098718";
                    options.AppSecret = "b157063900254d880c1a598be5130f72";
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = "Cag8MoeWpSBy1iOEiuyeOzrUd";
                    options.ConsumerSecret = "QD6M8CtqzyjPo6VrLGivLM8nBUEyJr8KzAYDWZ26ub3diBtPBF";
                });
                //.AddMicrosoftAccount(options => {
                //    options.ClientId = "";
                //    options.ClientSecret = "";
                //});

            services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role","true"));

                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageRolesAndClaimsRequirement()));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role","true"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                //    {
                //        var user = context.User;
                //        return (user.IsInRole("Admin") && user.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true"))
                //        || user.IsInRole("Superadmin");
                //    }));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithRedirects("/Error/{0}");
            }

            //var options = new DefaultFilesOptions();
            //options.DefaultFileNames.Clear();
            //options.DefaultFileNames.Add("foo.html");
            //app.UseDefaultFiles(options);

            app.UseStaticFiles();

            app.UseAuthentication();

            //var options = new FileServerOptions();
            //options.EnableDirectoryBrowsing = true;
            //options.DefaultFilesOptions.DefaultFileNames.Clear();
            //options.DefaultFilesOptions.DefaultFileNames.Add("default.html");
            //app.UseFileServer(options);

            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
            //app.Run(async (context) =>
            //{
            //    //await context.Response.WriteAsync($"Hello World");
            //});
        }
    }
}