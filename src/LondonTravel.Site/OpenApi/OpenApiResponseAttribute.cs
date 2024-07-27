// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.OpenApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class OpenApiResponseAttribute(int httpStatusCode, string description) : Attribute
{
    public int HttpStatusCode { get; } = httpStatusCode;

    public string Description { get; } = description;
}
