// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class AddSecurity : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme()
        {
            BearerFormat = "Opaque token",
            Description = "Access token authentication using a bearer token.",
            Scheme = "bearer",
            Type = SecuritySchemeType.Http,
        };

        var reference = new OpenApiSecuritySchemeReference("Bearer", document);

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[reference.Reference.Id] = scheme;
        document.SecurityRequirements ??= [];
        document.SecurityRequirements.Add(new() { [reference] = [] });

        return Task.CompletedTask;
    }
}
