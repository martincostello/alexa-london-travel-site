// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Models;

namespace MartinCostello.LondonTravel.Site.Swagger;

/// <summary>
/// A class representing an implementation of <see cref="IExampleProvider"/>
/// for the <see cref="ErrorResponse"/> class. This class cannot be inherited.
/// </summary>
public sealed class ErrorResponseExampleProvider : IExampleProvider<ErrorResponse>
{
    /// <inheritdoc />
    public object GetExample()
    {
        return new ErrorResponse()
        {
            Message = "Unauthorized.",
            RequestId = "0HKT0TM6UJASI",
            StatusCode = 401,
            Details = ["Only the Bearer authorization scheme is supported."],
        };
    }
}
