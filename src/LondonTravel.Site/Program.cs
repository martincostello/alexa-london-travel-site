// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#pragma warning disable SA1516

using System.IO.Compression;
using System.Net.Mime;
using MartinCostello.LondonTravel.Site;
using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Services;
using MartinCostello.LondonTravel.Site.Services.Data;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using MartinCostello.LondonTravel.Site.Swagger;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureApplication();
builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Services.AddOptions();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<ApplicationInsightsServiceOptions>((p) => p.ApplicationVersion = GitMetadata.Commit);

#pragma warning disable CA1308
string environment = builder.Configuration.AzureEnvironment().ToLowerInvariant();
#pragma warning restore CA1308

var dataProtection = builder.Services
    .AddDataProtection()
    .SetApplicationName($"londontravel-{environment}");

string connectionString = builder.Configuration.AzureStorageConnectionString();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    string relativePath = $"/london-travel/{environment}/keys.xml";
    dataProtection.PersistKeysToAzureBlobStorage(
        connectionString,
        "data-protection",
        relativePath);
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
                .WithExposedHeaders(siteOptions?.Api?.Cors?.ExposedHeaders ?? Array.Empty<string>())
                .WithHeaders(siteOptions?.Api?.Cors?.Headers ?? Array.Empty<string>())
                .WithMethods(siteOptions?.Api?.Cors?.Methods ?? Array.Empty<string>());

            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(siteOptions?.Api?.Cors?.Origins ?? Array.Empty<string>());
            }
        });
});

builder.Services
    .AddLocalization()
    .AddControllersWithViews((options) =>
    {
        options.Conventions.Add(new ApiExplorerDisplayConvention());

        if (!builder.Environment.IsDevelopment())
        {
            options.Filters.Add(new RequireHttpsAttribute());
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger(builder.Environment);

builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression((options) =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>((options) =>
{
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddSingleton<IClock>((_) => SystemClock.Instance);
builder.Services.AddSingleton<ISiteTelemetry, SiteTelemetry>();
builder.Services.AddSingleton<ITelemetryInitializer, SiteTelemetryInitializer>();
builder.Services.AddSingleton<ITflServiceFactory, TflServiceFactory>();
builder.Services.AddSingleton((p) => DocumentHelpers.CreateClient(p));
builder.Services.TryAddSingleton<IDocumentService, DocumentService>();
builder.Services.TryAddSingleton<IDocumentCollectionInitializer, DocumentCollectionInitializer>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SiteResources>();

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<AlexaService>();
builder.Services.AddTransient<ITflService, TflService>();
builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue);
builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.Authentication!.UserStore!);
builder.Services.AddTransient((p) => p.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.Tfl!);

builder.Services.AddPolly();
builder.Services.AddHttpClients();

builder.Services.AddApplicationAuthentication(builder.Configuration);

var app = builder.Build();

app.Lifetime.ApplicationStopped.Register(() => Serilog.Log.CloseAndFlush());

var options = app.Services.GetRequiredService<IOptionsMonitor<SiteOptions>>();

app.UseCustomHttpHeaders(app.Environment, app.Configuration, options);

app.UseRequestLocalization("en-GB", "en-US");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error")
       .UseStatusCodePagesWithReExecute("/error", "?id={0}");
}

app.UseHsts()
   .UseHttpsRedirection();

app.UseResponseCompression();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    DefaultContentType = MediaTypeNames.Application.Json,
    OnPrepareResponse = (context) => SetCacheHeaders(context, app.Environment.IsDevelopment()),
    ServeUnknownFileTypes = true,
});

app.UseRouting();

app.UseIdentity(options.CurrentValue);

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.MapAlexa();
app.MapApi(app.Logger);
app.MapRedirects();

app.UseSwagger();

app.UseCookiePolicy(new()
{
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always,
});

app.Run();

static void SetCacheHeaders(StaticFileResponseContext context, bool isDevelopment)
{
    var maxAge = TimeSpan.FromDays(7);

    if (context.File.Exists && !isDevelopment)
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
}
