// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using System;
    using System.Globalization;
    using Extensions;
    using MartinCostello.LondonTravel.Site.Services;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.WindowsAzure.Storage;
    using Newtonsoft.Json;
    using NodaTime;
    using Options;
    using Services.Data;
    using Services.Tfl;
    using Telemetry;

    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the current hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="options">The snapshot of <see cref="SiteOptions"/> to use.</param>
        public void Configure(IApplicationBuilder app, IOptionsSnapshot<SiteOptions> options)
        {
            app.UseCustomHttpHeaders(HostingEnvironment, Configuration, options);

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

            if (HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error")
                   .UseStatusCodePagesWithReExecute("/error", "?id={0}");
            }

            app.UseHsts()
               .UseHttpsRedirection();

            app.UseStaticFiles(CreateStaticFileOptions());

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            app.UseHttpMethodOverride();

            app.UseIdentity(options.Value);

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();

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
            services.Configure<ApplicationInsightsServiceOptions>((p) => p.ApplicationVersion = GitMetadata.Commit);

            ConfigureDataProtection(services);

            services.AddAntiforgery(
                (p) =>
                {
                    p.Cookie.Name = ApplicationCookie.Antiforgery.Name;
                    p.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    p.FormFieldName = "_anti-forgery";
                    p.HeaderName = "x-anti-forgery";
                });

            services
                .AddMemoryCache()
                .AddDistributedMemoryCache()
                .AddCors(ConfigureCors);

            services
                .AddLocalization()
                .AddMvc(ConfigureMvc)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddJsonOptions((p) => ConfigureJsonFormatter(p));

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            services.AddHsts(
                (p) =>
                {
                    p.MaxAge = TimeSpan.FromDays(365);
                    p.IncludeSubDomains = false;
                    p.Preload = false;
                });

            services.AddSwagger(HostingEnvironment);

            services
                .AddResponseCaching()
                .AddResponseCompression();

            services.AddSingleton<IClock>((_) => SystemClock.Instance);
            services.AddSingleton<IDocumentCollectionInitializer, DocumentCollectionInitializer>();
            services.AddSingleton<IDocumentClient, DocumentClientWrapper>();
            services.AddSingleton<ISiteTelemetry, SiteTelemetry>();
            services.AddSingleton<ITelemetryInitializer, SiteTelemetryInitializer>();
            services.AddSingleton<ITelemetryModule, SiteTelemetryModule>();
            services.AddSingleton<ITflServiceFactory, TflServiceFactory>();
            services.AddSingleton((_) => ConfigureJsonFormatter(new JsonSerializerSettings()));

            services.AddScoped((p) => p.GetRequiredService<IHttpContextAccessor>().HttpContext);
            services.AddScoped((p) => p.GetRequiredService<IOptionsSnapshot<SiteOptions>>().Value);
            services.AddScoped((p) => p.GetRequiredService<SiteOptions>().Authentication.UserStore);
            services.AddScoped((p) => p.GetRequiredService<SiteOptions>().Tfl);

            services.AddScoped<SiteResources>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ITflService, TflService>();

            services.AddPolly();
            services.AddHttpClients();

            services.AddApplicationAuthentication();

            services.RemoveApplicationInsightsTagHelper();

            return ServiceProvider = services.BuildServiceProvider();
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
            return ConfigureJsonFormatter(options.SerializerSettings);
        }

        /// <summary>
        /// Configures the JSON serializer.
        /// </summary>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> to configure.</param>
        /// <returns>
        /// The <see cref="JsonSerializerSettings"/> to use.
        /// </returns>
        private static JsonSerializerSettings ConfigureJsonFormatter(JsonSerializerSettings settings)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            settings.Formatting = Formatting.Indented;

            // Omit nulls to reduce payload size
            settings.NullValueHandling = NullValueHandling.Ignore;

            // Explicitly define behavior when serializing DateTime values
            settings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision

            return settings;
        }

        private static StaticFileOptions CreateStaticFileOptions()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            return new StaticFileOptions()
            {
                ContentTypeProvider = provider,
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
            };
        }

        /// <summary>
        /// Configures CORS.
        /// </summary>
        /// <param name="corsOptions">The <see cref="CorsOptions"/> to configure.</param>
        private void ConfigureCors(CorsOptions corsOptions)
        {
            var siteOptions = ServiceProvider.GetService<SiteOptions>();

            corsOptions.AddPolicy(
                "DefaultCorsPolicy",
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
        /// Configures Data Protection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        private void ConfigureDataProtection(IServiceCollection services)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            string environment = Configuration.AzureEnvironment().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            var dataProtection = services
                .AddDataProtection()
                .SetApplicationName($"londontravel-{environment}");

            string connectionString = Configuration.AzureStorageConnectionString();
            string relativePath = $"/data-protection/london-travel/{environment}/keys.xml";

            if (!string.IsNullOrWhiteSpace(connectionString) &&
                CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount account))
            {
                dataProtection.PersistKeysToAzureBlobStorage(account, relativePath);
            }
        }

        /// <summary>
        /// Configures MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        private void ConfigureMvc(MvcOptions options)
        {
            options.Conventions.Add(new Swagger.ApiExplorerDisplayConvention());

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
                Secure = CookieSecurePolicy.Always,
            };
        }
    }
}
