// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Models;

namespace MartinCostello.LondonTravel.Site.Swagger;

/// <summary>
/// A class representing an implementation of <see cref="IExampleProvider"/>
/// for the <see cref="PreferencesResponse"/> class. This class cannot be inherited.
/// </summary>
public sealed class PreferencesResponseExampleProvider : IExampleProvider<PreferencesResponse>
{
    /// <inheritdoc />
    public object GetExample()
    {
        return new PreferencesResponse()
        {
            FavoriteLines = ["northern", "victoria"],
            UserId = "578a0443-2208-4fb3-8e33-92351e58b685",
        };
    }
}
