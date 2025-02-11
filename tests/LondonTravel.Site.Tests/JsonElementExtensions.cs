// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;

namespace MartinCostello.LondonTravel.Site;

internal static class JsonElementExtensions
{
    internal static int GetInt32(this JsonElement element, string propertyName)
        => element.GetProperty(propertyName).GetInt32();

    internal static string? GetString(this JsonElement element, string propertyName)
        => element.GetProperty(propertyName).GetString();

    internal static string?[] GetStringArray(this JsonElement element, string propertyName)
        => [.. element.GetProperty(propertyName).EnumerateArray().Select((p) => p.GetString())];
}
