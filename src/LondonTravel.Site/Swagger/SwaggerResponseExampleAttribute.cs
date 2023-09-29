// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger;

/// <summary>
/// Defines an example response for an API method. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwaggerResponseExampleAttribute"/> class.
/// </remarks>
/// <param name="responseType">The type of the response.</param>
/// <param name="exampleType">The type of the example.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class SwaggerResponseExampleAttribute(Type responseType, Type exampleType) : Attribute
{
    /// <summary>
    /// Gets the type of the response.
    /// </summary>
    public Type ResponseType { get; } = responseType;

    /// <summary>
    /// Gets the type of the example.
    /// </summary>
    public Type ExampleType { get; } = exampleType;
}
