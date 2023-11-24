// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing Swagger-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Adds Swagger to the services.
    /// </summary>
    /// <param name="value">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <returns>
    /// The value specified by <paramref name="value"/>.
    /// </returns>
    public static IServiceCollection AddSwagger(this IServiceCollection value, IWebHostEnvironment environment)
    {
        return value.AddSwaggerGen((options) =>
        {
            var provider = value.BuildServiceProvider();
            var siteOptions = provider.GetRequiredService<SiteOptions>();

            var terms = new UriBuilder()
            {
                Scheme = "https",
                Host = siteOptions.Metadata?.Domain!,
                Path = "terms-of-service/",
            };

            var info = new OpenApiInfo()
            {
                Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = new Uri(siteOptions.Metadata?.Repository!, UriKind.Absolute),
                },
                Description = siteOptions.Metadata?.Description,
                License = new()
                {
                    Name = "Apache 2.0",
                    Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html", UriKind.Absolute),
                },
                TermsOfService = terms.Uri,
                Title = siteOptions.Metadata?.Name,
                Version = string.Empty,
            };

            options.EnableAnnotations();

            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            AddXmlCommentsIfExists(options, environment, "LondonTravel.Site.xml");

            options.SwaggerDoc("api", info);

            options.SchemaFilter<ExampleFilter>();
            options.OperationFilter<ExampleFilter>();
            options.OperationFilter<RemoveStyleCopPrefixesFilter>();

            string schemeName = "Access Token";

            var securityScheme = new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Description = "Access token authentication using a bearer token.",
                Reference = new()
                {
                    Id = schemeName,
                    Type = ReferenceType.SecurityScheme,
                },
                UnresolvedReference = false,
            };

            var securityRequirement = new OpenApiSecurityRequirement()
            {
                [securityScheme] = Array.Empty<string>(),
            };

            options.AddSecurityDefinition(schemeName, securityScheme);
            options.AddSecurityRequirement(securityRequirement);
        });
    }

    /// <summary>
    /// Adds XML comments to Swagger if the file exists.
    /// </summary>
    /// <param name="options">The Swagger options.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <param name="fileName">The XML comments file name to try to add.</param>
    private static void AddXmlCommentsIfExists(SwaggerGenOptions options, IWebHostEnvironment environment, string fileName)
    {
        string? applicationPath;

        if (environment.IsDevelopment())
        {
            applicationPath = Path.GetDirectoryName(AppContext.BaseDirectory);
        }
        else
        {
            applicationPath = environment.ContentRootPath;
        }

        string path = Path.GetFullPath(Path.Combine(applicationPath ?? ".", fileName));

        if (File.Exists(path))
        {
            options.IncludeXmlComments(path);
        }
    }
}
