// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

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

        string referenceId = "Bearer";
        var reference = new OpenApiSecuritySchemeReference(referenceId, document);

        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[referenceId] = scheme;
        document.Security ??= [];
        document.Security.Add(new() { [reference] = [] });

        return Task.CompletedTask;
    }
}
