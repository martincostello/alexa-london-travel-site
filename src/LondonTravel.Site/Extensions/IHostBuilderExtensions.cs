// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace MartinCostello.LondonTravel.Site.Extensions;

internal static class IHostBuilderExtensions
{
    public static IHostBuilder ConfigureApplication(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, builder) =>
        {
            // Build the configuration so far, this ensures things like user secrets are available
            IConfiguration config = builder.Build();

            if (TryGetVaultUri(config, out var vaultUri))
            {
                var credential = CreateTokenCredential(config);
                var manager = new AzureEnvironmentSecretManager(config.AzureEnvironment());

                builder.AddAzureKeyVault(vaultUri, credential, manager);
            }
        });

        builder.ConfigureServices((services) =>
        {
            services.AddSingleton((provider) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();

                if (!TryGetVaultUri(config, out var vaultUri))
                {
                    return null!;
                }

                var credential = CreateTokenCredential(config);
                return new SecretClient(vaultUri, credential);
            });
        });

        return builder;
    }

    private static bool TryGetVaultUri(IConfiguration configuration, [NotNullWhen(true)] out Uri? vaultUri)
    {
        string vault = configuration["AzureKeyVault:Uri"] ?? string.Empty;

        if (!string.IsNullOrEmpty(vault) && Uri.TryCreate(vault, UriKind.Absolute, out vaultUri))
        {
            return true;
        }

        vaultUri = null;
        return false;
    }

    private static TokenCredential CreateTokenCredential(IConfiguration configuration)
    {
        string clientId = configuration["AzureKeyVault:ClientId"] ?? string.Empty;
        string clientSecret = configuration["AzureKeyVault:ClientSecret"] ?? string.Empty;
        string tenantId = configuration["AzureKeyVault:TenantId"] ?? string.Empty;

        bool useClientSecret =
            !string.IsNullOrEmpty(clientId) &&
            !string.IsNullOrEmpty(clientSecret) &&
            !string.IsNullOrEmpty(tenantId);

        // Use explicitly configured Azure Key Vault credentials, otherwise
        // Assume Managed Service Identity is configured and available.
        return
            useClientSecret ?
            new ClientSecretCredential(tenantId, clientId, clientSecret) :
            new ManagedIdentityCredential();
    }
}
