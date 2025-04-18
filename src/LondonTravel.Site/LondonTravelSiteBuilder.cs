// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Storage.Blobs;
using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Services;
using MartinCostello.LondonTravel.Site.Services.Data;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site;

public static class LondonTravelSiteBuilder
{
    public static WebApplicationBuilder AddLondonTravelSite(this WebApplicationBuilder builder)
    {
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
        {
            ExcludeVisualStudioCredential = true,
        });

        if (builder.Configuration["ConnectionStrings:AzureKeyVault"] is { Length: > 0 })
        {
            builder.Configuration.AddAzureKeyVaultSecrets("AzureKeyVault", (p) => p.Credential = credential);
        }

        if (builder.Configuration["ConnectionStrings:AzureBlobStorage"] is { Length: > 0 })
        {
            builder.AddAzureBlobClient("AzureBlobStorage", (p) => p.Credential = credential);
        }

        if (builder.Configuration["ConnectionStrings:AzureCosmos"] is { Length: > 0 })
        {
            builder.AddAzureCosmosClient(
                "AzureCosmos",
                (settings) => settings.Credential = credential,
                (options) =>
                {
                    options.ApplicationName = "london-travel";
                    options.RequestTimeout = TimeSpan.FromSeconds(15);
                    options.UseSystemTextJsonSerializerWithOptions = System.Text.Json.JsonSerializerOptions.Default;

                    if (builder.Configuration["Site:Authentication:UserStore:CurrentLocation"] is { Length: > 0 } region)
                    {
                        options.ApplicationRegion = region;
                    }
                });
        }

        builder.WebHost.CaptureStartupErrors(true);
        builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

        if (builder.Configuration["Sentry:Dsn"] is { Length: > 0 } dsn)
        {
            builder.WebHost.UseSentry(dsn);
        }

        builder.Services.AddOptions();
        builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

        builder.Logging.AddTelemetry();
        builder.Services.AddTelemetry(builder.Environment);

#pragma warning disable CA1308
        string environment = builder.Configuration.AzureEnvironment().ToLowerInvariant();
#pragma warning restore CA1308

        var dataProtection = builder.Services
            .AddDataProtection()
            .SetApplicationName($"londontravel-{environment}");

        if (builder.Configuration["ConnectionStrings:AzureBlobStorage"] is { Length: > 0 })
        {
            dataProtection.PersistKeysToAzureBlobStorage(static (provider) =>
            {
                var client = provider.GetRequiredService<BlobServiceClient>();
                var environment = provider.GetRequiredService<IHostEnvironment>();

                string containerName = "data-protection";
                string blobName = $"{environment}/keys.xml";

                if (environment.IsDevelopment() && !client.GetBlobContainers().Any((p) => p.Name == containerName))
                {
                    client.CreateBlobContainer(containerName);
                }

                return client.GetBlobContainerClient(containerName).GetBlobClient(blobName);
            });
        }

        builder.Services.AddAntiforgery((options) =>
        {
            options.Cookie.Name = ApplicationCookie.Antiforgery.Name;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.FormFieldName = "_anti-forgery";
            options.HeaderName = "x-anti-forgery";
        });

        builder.Services.AddCors((corsOptions) =>
        {
            var siteOptions = new SiteOptions();
            builder.Configuration.Bind("Site", siteOptions);

            corsOptions.AddPolicy(
                "DefaultCorsPolicy",
                (policy) =>
                {
                    policy
                        .WithExposedHeaders(siteOptions?.Api?.Cors?.ExposedHeaders ?? [])
                        .WithHeaders(siteOptions?.Api?.Cors?.Headers ?? [])
                        .WithMethods(siteOptions?.Api?.Cors?.Methods ?? []);

                    if (builder.Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(siteOptions?.Api?.Cors?.Origins ?? []);
                    }
                });
        });

        builder.Services
            .AddLocalization()
            .AddControllersWithViews((options) =>
            {
                if (!builder.Environment.IsDevelopment())
                {
                    options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute());
                }
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

        builder.Services.AddRazorPages();

        builder.Services.AddRouting((options) =>
        {
            options.AppendTrailingSlash = true;
            options.LowercaseUrls = true;
        });

        builder.Services.AddHsts((options) =>
        {
            options.IncludeSubDomains = false;
            options.MaxAge = TimeSpan.FromDays(365);
            options.Preload = false;
        });

        builder.Services.AddOpenApiDocumentation();
        builder.Services.AddOutputCache();

        builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
        builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

        builder.Services.AddResponseCaching();
        builder.Services.AddResponseCompression((options) =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.ConfigureHttpJsonOptions((options) =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.TypeInfoResolverChain.Add(ApplicationJsonSerializerContext.Default);
        });

        builder.Services.Configure<StaticFileOptions>((options) =>
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            options.ContentTypeProvider = provider;
            options.DefaultContentType = "application/json";
            options.ServeUnknownFileTypes = true;

            options.OnPrepareResponse = (context) =>
            {
                var maxAge = TimeSpan.FromDays(7);

                if (context.File.Exists && builder.Environment.IsProduction())
                {
                    string? extension = Path.GetExtension(context.File.PhysicalPath);

                    // These files are served with a content hash in the URL so can be cached for longer
                    bool isScriptOrStyle =
                        string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase);

                    if (isScriptOrStyle)
                    {
                        maxAge = TimeSpan.FromDays(365);
                    }
                }

                var headers = context.Context.Response.GetTypedHeaders();
                headers.CacheControl = new() { MaxAge = maxAge };
            };
        });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ITflServiceFactory, TflServiceFactory>();

        builder.Services.TryAddSingleton<IDocumentService, DocumentService>();
        builder.Services.TryAddSingleton<IDocumentCollectionInitializer, DocumentCollectionInitializer>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<SiteResources>();

        builder.Services.AddTransient<IAccountService, AccountService>();
        builder.Services.AddTransient<AlexaService>();
        builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue);
        builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.Authentication!.UserStore!);
        builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.Tfl!);

        builder.Services.AddHttpClients();

        builder.Services.AddApplicationAuthentication(builder.Configuration);

        return builder;
    }

    public static WebApplication UseLondonTravelSite(this WebApplication app)
    {
        if (ApplicationTelemetry.IsPyroscopeConfigured())
        {
            app.UseMiddleware<Middleware.PyroscopeK6Middleware>();
        }

        var options = app.Services.GetRequiredService<IOptionsMonitor<SiteOptions>>();

        app.UseCustomHttpHeaders(app.Environment, app.Configuration, options);

        app.UseRequestLocalization("en-GB", "en-US");

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error")
               .UseStatusCodePagesWithReExecute("/error", "?id={0}");

            app.UseHsts();

            if (!string.Equals(app.Configuration["ForwardedHeaders_Enabled"], bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                app.UseHttpsRedirection();
            }
        }

        app.UseResponseCompression();

        app.UseStaticFiles();

        app.UseIdentity(options.CurrentValue);

        app.MapDefaultControllerRoute();
        app.MapRazorPages();

        app.MapAlexa();
        app.MapApi();
        app.MapRedirects();

        app.UseOutputCache();
        app.MapOpenApi().CacheOutput();

        app.UseCookiePolicy(new()
        {
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = CookieSecurePolicy.Always,
        });

        return app;
    }
}
