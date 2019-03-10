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
    using Options;
    using Swashbuckle.AspNetCore.Swagger;
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
        public static IServiceCollection AddSwagger(this IServiceCollection value, IHostingEnvironment environment)
        {
            return value.AddSwaggerGen(
                (p) =>
                {
                    var provider = value.BuildServiceProvider();
                    var options = provider.GetRequiredService<SiteOptions>();

                    var terms = new UriBuilder()
                    {
                        Scheme = "https",
                        Host = options.Metadata.Domain,
                        Path = "terms-of-service/",
                    };

                    var info = new Info()
                    {
                        Contact = new Contact()
                        {
                            Name = options.Metadata.Author.Name,
                            Url = options.Metadata.Repository,
                        },
                        Description = options.Metadata.Description,
                        License = new License()
                        {
                            Name = "Apache 2.0",
                            Url = "https://www.apache.org/licenses/LICENSE-2.0.html",
                        },
                        TermsOfService = terms.Uri.ToString(),
                        Title = options.Metadata.Name,
                        Version = string.Empty,
                    };

                    p.DescribeAllEnumsAsStrings();
                    p.DescribeStringEnumsInCamelCase();

                    p.IgnoreObsoleteActions();
                    p.IgnoreObsoleteProperties();

                    AddXmlCommentsIfExists(p, environment, "LondonTravel.Site.xml");

                    p.SwaggerDoc("api", info);

                    p.SchemaFilter<ExampleFilter>();

                    p.OperationFilter<ExampleFilter>();
                    p.OperationFilter<RemoveStyleCopPrefixesFilter>();
                    p.OperationFilter<SecurityRequirementsOperationFilter>();

                    var securityScheme = new ApiKeyScheme()
                    {
                        In = "header",
                        Name = "Authorization",
                        Type = "apiKey",
                        Description = "Access token authentication using a bearer token."
                    };

                    p.AddSecurityDefinition(SecurityRequirementsOperationFilter.SchemeName, securityScheme);
                });
        }

        /// <summary>
        /// Adds XML comments to Swagger if the file exists.
        /// </summary>
        /// <param name="options">The Swagger options.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <param name="fileName">The XML comments file name to try to add.</param>
        private static void AddXmlCommentsIfExists(SwaggerGenOptions options, IHostingEnvironment environment, string fileName)
        {
            var modelType = typeof(Startup).GetTypeInfo();
            string applicationPath;

            if (environment.IsDevelopment())
            {
                applicationPath = Path.GetDirectoryName(modelType.Assembly.Location);
            }
            else
            {
                applicationPath = environment.ContentRootPath;
            }

            var path = Path.GetFullPath(Path.Combine(applicationPath, fileName));

            if (File.Exists(path))
            {
                options.IncludeXmlComments(path);
            }
        }
    }
}
