// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter. This class cannot be inherited.
/// </summary>
/// <param name="value">The example value.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class OpenApiExampleAttribute(string value) : OpenApiExampleAttribute<string, OpenApiExampleAttribute>, IExampleProvider<string>
{
    /// <summary>
    /// Gets the example value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc/>
    static string IExampleProvider<string>.GenerateExample() => "string";

    /// <inheritdoc />
    public override string GenerateExample() => Value;
}
