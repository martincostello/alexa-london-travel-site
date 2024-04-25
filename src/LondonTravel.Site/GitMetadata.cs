// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class containing Git metadata for the assembly. This class cannot be inherited.
/// </summary>
public static class GitMetadata
{
    /// <summary>
    /// Gets the SHA for the Git branch the assembly was compiled from.
    /// </summary>
    public static string Branch { get; } = GetMetadataValue("CommitBranch", "Unknown");

    /// <summary>
    /// Gets the SHA for the Git commit the assembly was compiled from.
    /// </summary>
    public static string Commit { get; } = GetMetadataValue("CommitHash", "HEAD");

    /// <summary>
    /// Gets the Id for the GitHub Actions run the assembly was compiled and deployed from.
    /// </summary>
    public static string DeployId { get; } = GetMetadataValue("DeployId", "Unknown");

    /// <summary>
    /// Gets the timestamp the assembly was compiled at.
    /// </summary>
    public static DateTime Timestamp { get; } = DateTime.Parse(GetMetadataValue("BuildTimestamp", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    public static string Version { get; } = typeof(GitMetadata).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    /// <summary>
    /// Gets the specified metadata value.
    /// </summary>
    /// <param name="name">The name of the metadata value to retrieve.</param>
    /// <param name="defaultValue">The default value if the metadata is not found.</param>
    /// <returns>
    /// A <see cref="string"/> containing the Git SHA-1 for the revision of the application.
    /// </returns>
    private static string GetMetadataValue(string name, string defaultValue)
    {
        return typeof(GitMetadata).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where((p) => string.Equals(p.Key, name, StringComparison.Ordinal))
            .Select((p) => p.Value)
            .FirstOrDefault() ?? defaultValue;
    }
}
