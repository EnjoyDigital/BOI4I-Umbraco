using AspNetCore.Unobtrusive.Ajax;
using BOI.Core.Middleware;
using BOI.Core.Web.Extensions;
using BOI.Core.Web.Models.Cors;
using Microsoft.AspNetCore.Rewrite;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using static BOI.Core.Web.Constants.SiteAliases;


namespace BOI.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web hosting environment.</param>
        /// <param name="config">The configuration.</param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337.
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            var corsConfig = _config.GetSection("CorsConfig").Get<CorsConfig>();
            if (corsConfig != null)
            {
                services = services.AddCors(options =>
                {
                    options.AddPolicy(Cors.CorsPolicyName, builder =>
                    {
                        foreach (var corsItem in corsConfig.Origins)
                        {
                            builder.WithOrigins(corsItem.RequestOrigin);
                        }
                    }
                    );

                }

                );

            }
            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddCustomUmbracoDataContext()
                .AddServerRegistrar(_config)
                .AddCustomServices(_config)
                .AddDataProtection(_config)
                .AddCustomNotificationHandlers()
                .AddCustomContentFinders()
                .AddComposers()
                .RegisterQueryHandlers(_config)
                .Build();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            UseCustomRewrites(app, env, _config, logger);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(err => err.UseCustomErrors(env));
            }

            app.UseUmbraco()
                .WithMiddleware(u =>
                {
                    u.UseBackOffice();
                    u.UseWebsite();
                })
                .WithEndpoints(u =>
                {
                    u.UseCustomRoutes();
                    u.UseInstallerEndpoints();
                    u.UseBackOfficeEndpoints();
                    u.UseWebsiteEndpoints();
                });
            //app.UseMiddleware<MediaRequestHandler>();
            app.UseStaticFiles();

            app.UseUnobtrusiveAjax();
        }

        private void UseCustomRewrites(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            var options = new RewriteOptions();

            using (var iisUrlRewriteStreamReader = File.OpenText("Config/rewrites.config"))
            {
                options.AddIISUrlRewrite(iisUrlRewriteStreamReader);
            }

            if (!env.IsDevelopment())
            {
                app.UseRewriter(options);
            }
        }

        //return app;
    }

}
