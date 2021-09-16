// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace MartinCostello.LondonTravel.Site.Identity;

/// <summary>
/// A class representing external login information for a user.
/// </summary>
public class LondonTravelLoginInfo
{
    /// <summary>
    /// Gets or sets the login provider.
    /// </summary>
    [JsonProperty("loginProvider")]
    [JsonPropertyName("loginProvider")]
    public string? LoginProvider { get; set; }

    /// <summary>
    /// Gets or sets the provider key.
    /// </summary>
    [JsonProperty("providerKey")]
    [JsonPropertyName("providerKey")]
    public string? ProviderKey { get; set; }

    /// <summary>
    /// Gets or sets the provider display name.
    /// </summary>
    [JsonProperty("providerDisplayName")]
    [JsonPropertyName("providerDisplayName")]
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    /// Created an instance of <see cref="LondonTravelLoginInfo"/> from the specified <see cref="UserLoginInfo"/>.
    /// </summary>
    /// <param name="info">The <see cref="UserLoginInfo"/> to use to create the instance.</param>
    /// <returns>
    /// An instance of <see cref="LondonTravelLoginInfo"/> created from <paramref name="info"/>.
    /// </returns>
    internal static LondonTravelLoginInfo FromUserLoginInfo(UserLoginInfo info)
    {
        return new LondonTravelLoginInfo()
        {
            LoginProvider = info.LoginProvider,
            ProviderDisplayName = info.ProviderDisplayName,
            ProviderKey = info.ProviderKey,
        };
    }
}
