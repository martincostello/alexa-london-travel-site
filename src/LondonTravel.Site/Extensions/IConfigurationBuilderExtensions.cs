// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Azure.Core;
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// A class containing extension methods for the <see cref="IConfigurationBuilder"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IConfigurationBuilderExtensions
    {
        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to configure.</param>
        /// <param name="context">The <see cref="HostBuilderContext"/> to use.</param>
        /// <returns>
        /// The <see cref="IConfigurationBuilder"/> passed as the value of <paramref name="builder"/>.
        /// </returns>
        public static IConfigurationBuilder ConfigureApplication(this IConfigurationBuilder builder, HostBuilderContext context)
        {
            builder.AddApplicationInsightsSettings(developerMode: context.HostingEnvironment.IsDevelopment());

            // Build the configuration so far, this ensures things like user secrets are available
            IConfiguration config = builder.Build();

            if (TryGetVaultUri(config, out Uri? vaultUri))
            {
                var credential = CreateTokenCredential(config);
                var manager = new AzureEnvironmentSecretManager(config.AzureEnvironment());

                builder.AddAzureKeyVault(vaultUri, credential, manager);
            }

            return builder;
        }

        private static bool TryGetVaultUri(IConfiguration configuration, [NotNullWhen(true)] out Uri? vaultUri)
        {
            string vault = configuration["AzureKeyVault:Uri"];

            if (!string.IsNullOrEmpty(vault) && Uri.TryCreate(vault, UriKind.Absolute, out vaultUri))
            {
                return true;
            }

            vaultUri = null;
            return false;
        }

        private static TokenCredential CreateTokenCredential(IConfiguration configuration)
        {
            string clientId = configuration["AzureKeyVault:ClientId"];
            string clientSecret = configuration["AzureKeyVault:ClientSecret"];
            string tenantId = configuration["AzureKeyVault:TenantId"];

            if (!string.IsNullOrEmpty(clientId) &&
                !string.IsNullOrEmpty(clientSecret) &&
                !string.IsNullOrEmpty(tenantId))
            {
                // Use explicitly configured Azure Key Vault credentials
                return new ClientSecretCredential(tenantId, clientId, clientSecret);
            }
            else
            {
                // Assume Managed Service Identity is configured and available
                return new ManagedIdentityCredential();
            }
        }
    }
}
