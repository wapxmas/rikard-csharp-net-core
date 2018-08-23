using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RikardWeb.Controllers;
using Microsoft.Extensions.Configuration;
using RikardWeb.Options;
using RikardLib.AspLog;
using RikardWeb.Services;
using Microsoft.Extensions.WebEncoders;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using RikardLib.AspCron;
using RikardWeb.Lib.News;
using RikardLib.Log;
using RikardWeb.Lib.Db.Options;
using RikardWeb.Lib.News.Services;
using RikardWeb.Lib.Db.Service;
using Microsoft.AspNetCore.Identity;
using RikardWeb.Lib.Identity;
using RikardLib.AspSmsRu.SmsRu.Options;
using SmsRu;
using RikardWeb.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp;
using RikardWeb.Lib.Adverts.Options;
using RikardWeb.Lib.Adverts.Service;
using RikardWeb.Lib.Adverts;

namespace RikardWeb
{
    public class Startup
    {
        private Logger logger = new Logger();

        public IConfigurationRoot Configuration { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("rikard_settings.json", optional: true);
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<InfoOptions>(Configuration.GetSection("Info"));
            services.Configure<DatabaseOptions>(Configuration.GetSection("Database"));
            services.Configure<SmsRuOptions>(Configuration.GetSection("SmsRu"));
            services.Configure<AdvertsOptions>(Configuration.GetSection("Adverts"));

            services.AddAspLogger();
            services.AddSendEmailService();
            services.AddSmsRu();
            services.AddMongoDbService();
            services.AddNewsDatabaseService();
            services.AddAdvertsDatabaseService();
            services.AddCron();
            services.AddMongoIdentity();

            services.Configure<IdentityOptions>(o =>
            {
                o.SignIn.RequireConfirmedEmail = true;
                o.SignIn.RequireConfirmedPhoneNumber = true;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 5;
                o.User.RequireUniqueEmail = true;
                o.User.AllowedUserNameCharacters = string.Empty;
                o.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                o.Cookies.ApplicationCookie.AutomaticChallenge = true;
                o.Cookies.ApplicationCookie.LoginPath = "/Profile/Login";
                o.Cookies.ApplicationCookie.AccessDeniedPath = "/error/403";
            });

            services.AddMvc()
                .AddRazorOptions(options => 
                    options.ParseOptions = new CSharpParseOptions(LanguageVersion.CSharp7));
            services.AddViewRender();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.CookieName = ".RikardRu.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(10);
            });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("Paid", pb => pb.AddRequirements(new IsUserPaid(new TimeSpan(1, 0, 0, 0))));
            });
            services.AddSingleton<IAuthorizationHandler, IsUserPadAuthorizationHandler>();

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                //loggerFactory.AddAspLogger();
                loggerFactory.AddConsole();
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseExceptionHandler("/error/server_exception");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            app.UseStaticFiles();
            app.UseIdentity();
            app.UseSession();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "newsList",
                    template: "News/NewsList/{page:int}",
                    defaults: new { controller = "News", action=nameof(NewsController.NewsList) });

                routes.MapRoute(
                    name: "newsArticle",
                    template: "News/NewsArticle/{article:guid}",
                    defaults: new { controller = "News", action = nameof(NewsController.NewsArticle) });

                routes.MapRoute(
                    name: "errors",
                    template: "error/{errid}",
                    defaults: new { controller = "Errors", action = "ShowError" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}",
                    defaults: new { controller = "Adverts", action = "RentList" });
            });

            /**
             * Automatically start services
             **/
            app.ApplicationServices.GetService<ISendEmailService>();
            app.ApplicationServices.GetService<IMongoDbService>();
            app.ApplicationServices.GetService<INewsDatabaseService>();
            app.ApplicationServices.GetService<IAdvertsDatabaseService>();
            app.ApplicationServices.GetService<IUserStore<IdentityUser>>();
            app.ApplicationServices.GetService<IRoleStore<IdentityRole>>();
            app.ApplicationServices.GetService<ISmsRuService>();
            app.ApplicationServices.GetService<IUsersService<IdentityUser>>();

            /**
             * Initialize cron service
             **/
            var cron = app.ApplicationServices.GetService<ICronService>();

            if(!env.IsDevelopment())
            {
                //cron.AddJob("*/5 * * * *", () => Task.Run(async () => await
                //    NewsUpdater.RunNewsUpdater()).GetAwaiter().GetResult(), "NewsUpdate");
                //cron.AddJob("*/1 * * * *", () => Task.Run(async () => await
                //    NewsTweeter.RunNewsTweeter()).GetAwaiter().GetResult(), "NewsTweeter");
                cron.AddJob("0 */6 * * *", () => {
                    if(AdvertsDatabase.Instance != null)
                    {
                        AdvertsDatabase.Instance.RunAdvertsUpdater();
                    }
                }, "AdvertsUpdate");
            }
        }
    }
}
