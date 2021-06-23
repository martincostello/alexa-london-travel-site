// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace MartinCostello.LondonTravel.Site
{
    /// <summary>
    /// A class representing an implementation of <see cref="KeyVaultSecretManager"/>
    /// that selects keys based on the Azure environment name. This class cannot be inherited.
    /// </summary>
    internal sealed class AzureEnvironmentSecretManager : KeyVaultSecretManager
    {
        /// <summary>
        /// The secret prefix to use for the environment.
        /// </summary>
        private readonly string _prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureEnvironmentSecretManager"/> class.
        /// </summary>
        /// <param name="azureEnvironment">The name of the Azure environment.</param>
        public AzureEnvironmentSecretManager(string azureEnvironment)
        {
            _prefix = $"LondonTravel-{azureEnvironment}-";
        }

        /// <inheritdoc />
        public override string GetKey(KeyVaultSecret secret)
        {
            return secret.Name.Substring(_prefix.Length)
                .Replace("--", "_", StringComparison.Ordinal)
                .Replace("-", ":", StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Load(SecretProperties secret)
        {
            return secret.Name.StartsWith(_prefix, StringComparison.Ordinal);
        }
    }
}
