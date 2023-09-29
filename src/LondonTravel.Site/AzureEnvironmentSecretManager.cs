// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class representing an implementation of <see cref="KeyVaultSecretManager"/>
/// that selects keys based on the Azure environment name. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AzureEnvironmentSecretManager"/> class.
/// </remarks>
/// <param name="azureEnvironment">The name of the Azure environment.</param>
internal sealed class AzureEnvironmentSecretManager(string azureEnvironment) : KeyVaultSecretManager
{
    /// <summary>
    /// The secret prefix to use for the environment.
    /// </summary>
    private readonly string _prefix = $"LondonTravel-{azureEnvironment}-";

    /// <inheritdoc />
    public override string GetKey(KeyVaultSecret secret)
    {
        return secret.Name[_prefix.Length..]
            .Replace("--", "_", StringComparison.Ordinal)
            .Replace("-", ":", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Load(SecretProperties secret)
    {
        return secret.Name.StartsWith(_prefix, StringComparison.Ordinal);
    }
}
