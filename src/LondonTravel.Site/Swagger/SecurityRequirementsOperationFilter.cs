// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger
{
    using System;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public const string SchemeName = "Access Token";

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // As we only expose one operation (and it is not decorated with [Authorize]),
            // just apply the Access Token requirement straight onto the operation.
            operation.Security = new[]
            {
                new OpenApiSecurityRequirement()
                {
                    { new OpenApiSecurityScheme() { Name = SchemeName }, Array.Empty<string>() },
                }
            };
        }
    }
}
