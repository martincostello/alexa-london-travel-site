// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class representing the number of documents in the document store. This class cannot be inherited.
/// </summary>
public sealed class DocumentCount
{
    /// <summary>
    /// Gets or sets the number of documents in the store.
    /// </summary>
    [JsonPropertyName("count")]
    public long Count { get; set; }
}
