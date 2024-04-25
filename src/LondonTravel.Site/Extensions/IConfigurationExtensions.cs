// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for <see cref="IConfiguration"/>. This class cannot be inherited.
/// </summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Gets the name of the Azure environment the application is running in.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
    /// <returns>
    /// The name of the Azure environment the application is running in.
    /// </returns>
    public static string AzureEnvironment(this IConfiguration config)
    {
        return config?["Azure:Environment"] ?? "local";
    }

    /// <summary>
    /// Gets the connection string for the Azure storage account.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
    /// <returns>
    /// The connection string for the Azure storage account, if any.
    /// </returns>
    public static string AzureStorageConnectionString(this IConfiguration config)
    {
        return config?["ConnectionStrings:AzureStorage"] ?? string.Empty;
    }
}
