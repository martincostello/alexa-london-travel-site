// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.IO;
    using System.Reflection;
    using MartinCostello.LondonTravel.Site.Swagger;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Options;
    using Swashbuckle.AspNetCore.SwaggerGen;

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
            return value.AddSwaggerGen(
                (p) =>
                {
                    var provider = value.BuildServiceProvider();
                    var options = provider.GetRequiredService<SiteOptions>();

                    var terms = new UriBuilder()
                    {
                        Scheme = "https",
                        Host = options.Metadata?.Domain!,
                        Path = "terms-of-service/",
                    };

                    var info = new OpenApiInfo()
                    {
                        Contact = new OpenApiContact()
                        {
                            Name = options.Metadata?.Author?.Name,
                            Url = new Uri(options.Metadata?.Repository!, UriKind.Absolute),
                        },
                        Description = options.Metadata?.Description,
                        License = new OpenApiLicense()
                        {
                            Name = "Apache 2.0",
                            Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html", UriKind.Absolute),
                        },
                        TermsOfService = terms.Uri,
                        Title = options.Metadata?.Name,
                        Version = string.Empty,
                    };

                    p.EnableAnnotations();

                    p.IgnoreObsoleteActions();
                    p.IgnoreObsoleteProperties();

                    AddXmlCommentsIfExists(p, environment, "LondonTravel.Site.xml");

                    p.SwaggerDoc("api", info);

                    p.SchemaFilter<ExampleFilter>();
                    p.OperationFilter<ExampleFilter>();
                    p.OperationFilter<RemoveStyleCopPrefixesFilter>();

                    string schemeName = "Access Token";

                    var securityScheme = new OpenApiSecurityScheme()
                    {
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Description = "Access token authentication using a bearer token.",
                        Reference = new OpenApiReference()
                        {
                            Id = schemeName,
                            Type = ReferenceType.SecurityScheme,
                        },
                        UnresolvedReference = false,
                    };

                    var securityRequirement = new OpenApiSecurityRequirement()
                    {
                        { securityScheme, Array.Empty<string>() },
                    };

                    p.AddSecurityDefinition(schemeName, securityScheme);
                    p.AddSecurityRequirement(securityRequirement);
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
            var modelType = typeof(Startup).GetTypeInfo();
            string? applicationPath;

            if (environment.IsDevelopment())
            {
                applicationPath = Path.GetDirectoryName(modelType.Assembly.Location);
            }
            else
            {
                applicationPath = environment.ContentRootPath;
            }

            var path = Path.GetFullPath(Path.Combine(applicationPath ?? ".", fileName));

            if (File.Exists(path))
            {
                options.IncludeXmlComments(path);
            }
        }
    }
}
