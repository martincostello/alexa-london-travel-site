// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Azure.Core;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    internal static class IHostBuilderExtensions
    {
        public static IHostBuilder ConfigureApplication(this IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddApplicationInsightsSettings(developerMode: context.HostingEnvironment.IsDevelopment());

                // Build the configuration so far, this ensures things like user secrets are available
                IConfiguration config = builder.Build();

                if (TryGetVaultUri(config, out Uri? vaultUri))
                {
                    TokenCredential credential = CreateCredential(config);
                    builder.AddAzureKeyVault(vaultUri, credential);
                }
            });

            builder.ConfigureLogging((context, builder) => builder.ConfigureLogging(context));

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton((provider) =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();

                    if (!TryGetVaultUri(config, out Uri? vaultUri))
                    {
                        return null!;
                    }

                    TokenCredential credential = CreateCredential(config);
                    return new SecretClient(vaultUri, credential);
                });
            });

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

        private static TokenCredential CreateCredential(IConfiguration configuration)
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
