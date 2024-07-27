// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;

#pragma warning disable IDE0130
namespace Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Represents a transformer that can be used to modify an OpenAPI operation.
/// </summary>
internal interface IOpenApiOperationTransformer
{
    /// <summary>
    /// Transforms the specified OpenAPI operation.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
    /// <param name="context">The <see cref="OpenApiOperationTransformerContext"/> associated with the <see paramref="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
    Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken);
}
