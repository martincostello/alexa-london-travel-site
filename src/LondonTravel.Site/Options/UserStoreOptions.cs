// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options;

/// <summary>
/// A class representing the authentication user store options for the site. This class cannot be inherited.
/// </summary>
public sealed class UserStoreOptions
{
    /// <summary>
    /// Gets or sets the name of the Azure Cosmos DB database to use.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure Cosmos DB collection to use.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the current Azure Cosmos DB location, if any.
    /// </summary>
    public string? CurrentLocation { get; set; }
}
