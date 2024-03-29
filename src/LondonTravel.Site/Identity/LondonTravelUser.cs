// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MartinCostello.LondonTravel.Site.Identity;

/// <summary>
/// A class representing a user of the application.
/// </summary>
public class LondonTravelUser
{
    /// <summary>
    /// Gets or sets the user Id.
    /// </summary>
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ETag of the underlying document.
    /// </summary>
    [JsonProperty("_etag")]
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [JsonProperty("email")]
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the normalized email address.
    /// </summary>
    [JsonProperty("emailNormalized")]
    [JsonPropertyName("emailNormalized")]
    public string? EmailNormalized { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been confirmed.
    /// </summary>
    [JsonProperty("emailConfirmed")]
    [JsonPropertyName("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the user's given name.
    /// </summary>
    [JsonProperty("givenName")]
    [JsonPropertyName("givenName")]
    public string? GivenName { get; set; }

    /// <summary>
    /// Gets or sets the user's surname.
    /// </summary>
    [JsonProperty("surname")]
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    [JsonProperty("userName")]
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalized user name.
    /// </summary>
    [JsonProperty("userNameNormalized")]
    [JsonPropertyName("userNameNormalized")]
    public string? UserNameNormalized { get; set; }

    /// <summary>
    /// Gets or sets the user's external logins.
    /// </summary>
    [JsonProperty("logins")]
    [JsonPropertyName("logins")]
    public IList<LondonTravelLoginInfo> Logins { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's role claims.
    /// </summary>
    [JsonProperty("roleClaims")]
    [JsonPropertyName("roleClaims")]
    public IList<LondonTravelRole> RoleClaims { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's favorite line Ids.
    /// </summary>
    [JsonProperty("favoriteLines")]
    [JsonPropertyName("favoriteLines")]
    public IList<string> FavoriteLines { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's Amazon Alexa access token.
    /// </summary>
    [JsonProperty("alexaToken")]
    [JsonPropertyName("alexaToken")]
    public string? AlexaToken { get; set; }

    /// <summary>
    /// Gets or sets the user's security stamp.
    /// </summary>
    [JsonProperty("securityStamp")]
    [JsonPropertyName("securityStamp")]
    public string? SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the date and time the user was created.
    /// </summary>
    [JsonProperty("createdAt")]
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp of the user document.
    /// </summary>
    [JsonProperty("_ts")]
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}
