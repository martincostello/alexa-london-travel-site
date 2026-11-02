// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Identity;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IdentityResult"/> class. This class cannot be inherited.
/// </summary>
public static class IdentityResultExtensions
{
    /// <summary>
    /// Formats the errors of the specified <see cref="IdentityResult"/> for logging.
    /// </summary>
    /// <param name="result">The result to format the errors of.</param>
    /// <returns>
    /// A string containing the errors associated with <paramref name="result"/>.
    /// </returns>
    public static string FormatErrors(this IdentityResult result)
        => string.Join(';', result.Errors.Select((p) => $"{p.Code}: {p.Description}"));
}
