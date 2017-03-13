// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Extensions;
    using Identity;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Options;
    using Services.Data;
    using Services.Tfl;

    /// <summary>
    /// A class representing the base class for startup logic for the application.
    /// </summary>
    public abstract class StartupBase
    {
        /// <summary>
        /// The name of the default CORS policy.
        /// </summary>
        internal const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupBase"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        protected StartupBase(IHostingEnvironment env)
        {
            HostingEnvironment = env;
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the current hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="options">The snapshot of <see cref="SiteOptions"/> to use.</param>
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment environment,
            ILoggerFactory loggerFactory,
            IOptionsSnapshot<SiteOptions> options)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            app.UseCustomHttpHeaders(environment, Configuration, options);

            var supportedCultures = new[]
            {
                new CultureInfo("en-GB"),
                new CultureInfo("en-US"),
            };

            app.UseRequestLocalization(
                new RequestLocalizationOptions()
                {
                    DefaultRequestCulture = new RequestCulture("en-GB"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,
                });

            if (environment.IsDevelopment())
            {
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error")
                   .UseStatusCodePagesWithReExecute("/error", "?id={0}");
            }

            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    DefaultContentType = "application/json",
                    ServeUnknownFileTypes = true,
                    OnPrepareResponse = (context) =>
                        {
                            var headers = context.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue()
                            {
                                MaxAge = TimeSpan.FromDays(7)
                            };
                        }
                });

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            app.UseHttpMethodOverride();

            app.UseIdentity(options.Value, loggerFactory);

            app.UseMvc(
                (routes) =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });

            app.UseCookiePolicy(CreateCookiePolicy());
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddOptions();
            services.Configure<SiteOptions>(Configuration.GetSection("Site"));

            services.AddAntiforgery(
                (p) =>
                {
                    p.CookieName = "_anti-forgery";
                    p.FormFieldName = "_anti-forgery";
                    p.HeaderName = "x-anti-forgery";
                    p.RequireSsl = !HostingEnvironment.IsDevelopment();
                });

            services
                .AddMemoryCache()
                .AddDistributedMemoryCache()
                .AddCors(ConfigureCors);

            services.AddIdentity<LondonTravelUser, LondonTravelRole>(
                (options) =>
                {
                    options.Cookies.ApplicationCookie.AccessDeniedPath = "/account/access-denied/";
                    options.Cookies.ApplicationCookie.CookieName = "london-travel-auth-app";
                    options.Cookies.ApplicationCookie.CookieHttpOnly = true;
                    options.Cookies.ApplicationCookie.CookieSecure = CookiePolicy();
                    options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                    options.Cookies.ApplicationCookie.LoginPath = "/account/sign-in/";
                    options.Cookies.ApplicationCookie.LogoutPath = "/account/sign-out/";
                    options.Cookies.ApplicationCookie.SlidingExpiration = true;

                    (options.Cookies.ApplicationCookie.Events as CookieAuthenticationEvents).OnRedirectToLogin =
                        (p) =>
                        {
                            // Only redirect to the sign-in page if a non-API request
                            if (!p.Request.Path.StartsWithSegments("/api"))
                            {
                                p.Response.Redirect(p.RedirectUri);
                            }

                            return Task.CompletedTask;
                        };

                    options.Cookies.ExternalCookie.AccessDeniedPath = options.Cookies.ApplicationCookie.AccessDeniedPath;
                    options.Cookies.ExternalCookie.CookieName = "london-travel-auth-external";
                    options.Cookies.ExternalCookie.CookieHttpOnly = options.Cookies.ApplicationCookie.CookieHttpOnly;
                    options.Cookies.ExternalCookie.CookieSecure = options.Cookies.ApplicationCookie.CookieSecure;
                    options.Cookies.ExternalCookie.ExpireTimeSpan = options.Cookies.ApplicationCookie.ExpireTimeSpan;
                    options.Cookies.ExternalCookie.LoginPath = options.Cookies.ApplicationCookie.LoginPath;
                    options.Cookies.ExternalCookie.LogoutPath = options.Cookies.ApplicationCookie.LogoutPath;
                    options.Cookies.ExternalCookie.SlidingExpiration = options.Cookies.ApplicationCookie.SlidingExpiration;

                    options.User.RequireUniqueEmail = true;
                })
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
                .AddRoleStore<RoleStore>()
                .AddUserStore<UserStore>()
                .AddDefaultTokenProviders();

            services
                .AddLocalization()
                .AddMvc(ConfigureMvc)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddJsonOptions((p) => services.AddSingleton(ConfigureJsonFormatter(p)));

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            services
                .AddResponseCaching()
                .AddResponseCompression();

            services.AddSingleton<IConfiguration>((_) => Configuration);
            services.AddSingleton<IDocumentCollectionInitializer, DocumentCollectionInitializer>();
            services.AddSingleton<ITflServiceFactory, TflServiceFactory>();

            services.AddScoped((p) => p.GetRequiredService<IHttpContextAccessor>().HttpContext);
            services.AddScoped((p) => p.GetRequiredService<IOptionsSnapshot<SiteOptions>>().Value);
            services.AddScoped((p) => p.GetRequiredService<SiteOptions>().Authentication.UserStore);
            services.AddScoped((p) => p.GetRequiredService<SiteOptions>().Tfl);

            services.AddScoped<SiteResources>();
            services.AddScoped<IDocumentClient, DocumentClientWrapper>();

            services.AddTransient<HttpClient>();
            services.AddTransient<ITflService, TflService>();

            var builder = new ContainerBuilder();

            builder.Populate(services);

            var container = builder.Build();
            ServiceProvider = container.Resolve<IServiceProvider>();

            return ServiceProvider;
        }

        /// <summary>
        /// Configures the JSON serializer for MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcJsonOptions"/> to configure.</param>
        /// <returns>
        /// The <see cref="JsonSerializerSettings"/> to use.
        /// </returns>
        private static JsonSerializerSettings ConfigureJsonFormatter(MvcJsonOptions options)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            options.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls to reduce payload size
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Explicitly define behavior when serializing DateTime values
            options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision

            return options.SerializerSettings;
        }

        /// <summary>
        /// Configures CORS.
        /// </summary>
        /// <param name="corsOptions">The <see cref="CorsOptions"/> to configure.</param>
        private void ConfigureCors(CorsOptions corsOptions)
        {
            var siteOptions = ServiceProvider.GetService<SiteOptions>();

            corsOptions.AddPolicy(
                DefaultCorsPolicyName,
                (builder) =>
                {
                    builder
                        .WithExposedHeaders(siteOptions?.Api?.Cors?.ExposedHeaders ?? Array.Empty<string>())
                        .WithHeaders(siteOptions?.Api?.Cors?.Headers ?? Array.Empty<string>())
                        .WithMethods(siteOptions?.Api?.Cors?.Methods ?? Array.Empty<string>());

                    if (HostingEnvironment.IsDevelopment())
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(siteOptions?.Api?.Cors?.Origins ?? Array.Empty<string>());
                    }
                });
        }

        /// <summary>
        /// Configures MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        private void ConfigureMvc(MvcOptions options)
        {
            if (!HostingEnvironment.IsDevelopment())
            {
                options.Filters.Add(new RequireHttpsAttribute());
            }
        }

        /// <summary>
        /// Creates the <see cref="CookiePolicyOptions"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="CookiePolicyOptions"/> to use for the application.
        /// </returns>
        private CookiePolicyOptions CreateCookiePolicy()
        {
            return new CookiePolicyOptions()
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookiePolicy(),
            };
        }

        private CookieSecurePolicy CookiePolicy()
        {
            return HostingEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        }
    }
}
