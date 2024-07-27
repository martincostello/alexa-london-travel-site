// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.OpenApi;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing OpenAPI-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>
    /// The value specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();

        services.AddSwaggerGen((options) =>
        {
            using var provider = services.BuildServiceProvider();
            var siteOptions = provider.GetRequiredService<IOptions<SiteOptions>>().Value;

            var info = new OpenApiInfo()
            {
                Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = new(siteOptions.Metadata?.Repository ?? string.Empty),
                },
                Description = siteOptions.Metadata?.Description,
                License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0.html"),
                },
                TermsOfService = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = siteOptions.Metadata?.Domain!,
                    Path = "terms-of-service/",
                }.Uri,
                Title = siteOptions.Metadata?.Name,
                Version = string.Empty,
            };

            options.SwaggerDoc("api", info);

            options.EnableAnnotations();
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LondonTravel.Site.xml"));

            options.DocumentFilter<AddDocumentTags>();
            options.DocumentFilter<AddServers>();
            options.OperationFilter<AddDescriptions>();

            var examples = new AddExamples();
            options.AddOperationFilterInstance(examples);
            options.AddSchemaFilterInstance(examples);

            var prefixes = new RemoveStyleCopPrefixes();
            options.AddOperationFilterInstance(prefixes);
            options.AddSchemaFilterInstance(prefixes);

            var scheme = new OpenApiSecurityScheme()
            {
                BearerFormat = "Opaque token",
                Description = "Access token authentication using a bearer token.",
                Scheme = "bearer",
                Type = SecuritySchemeType.Http,
                Reference = new()
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme,
                },
            };

            options.AddSecurityDefinition(scheme.Reference.Id, scheme);
            options.AddSecurityRequirement(new() { [scheme] = [] });
        });

        return services;
    }

    private sealed class AddDocumentTags : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            => swaggerDoc.Tags.Add(new() { Name = "LondonTravel.Site" });
    }
}
