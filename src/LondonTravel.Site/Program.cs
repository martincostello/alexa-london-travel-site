// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using MartinCostello.LondonTravel.Site;
using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.OpenApi;
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

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureApplication();
builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

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

string connectionString = builder.Configuration.AzureStorageConnectionString();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    string relativePath = $"london-travel/{environment}/keys.xml";
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
        options.Conventions.Add(new ApiExplorerDisplayConvention());

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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
    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
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
builder.Services.AddSingleton(DocumentHelpers.CreateClient);
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

var app = builder.Build();

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

app.UseStaticFiles();

// HACK Workaround for https://github.com/dotnet/sdk/issues/40511
app.MapGet(".well-known/{fileName}", (string fileName, IWebHostEnvironment environment, IOptions<StaticFileOptions> options) =>
{
    var file = environment.WebRootFileProvider.GetFileInfo(Path.Combine("well-known", fileName));

    if (file.Exists && file.PhysicalPath is { Length: > 0 })
    {
        if (!options.Value.ContentTypeProvider.TryGetContentType(file.Name, out string? contentType) &&
            options.Value.ServeUnknownFileTypes)
        {
            contentType = options.Value.DefaultContentType;
        }

        return Results.File(file.PhysicalPath, contentType: contentType);
    }

    return Results.NotFound();
}).AllowAnonymous().ExcludeFromDescription();

app.UseRouting();

app.UseIdentity(options.CurrentValue);

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.MapAlexa();
app.MapApi();
app.MapRedirects();

app.UseOpenApi();

app.UseCookiePolicy(new()
{
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always,
});

app.Run();
