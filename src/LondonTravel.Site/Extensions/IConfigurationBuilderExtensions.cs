// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
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

            // Build the configuration so far
            IConfiguration config = builder.Build();

            // Get the settings for Azure Key Vault
            string vault = config["AzureKeyVault:Uri"];
            string clientId = config["AzureKeyVault:ClientId"];
            string clientSecret = config["AzureKeyVault:ClientSecret"];

            // Can Managed Service Identity be used instead of direct Key Vault integration?
            bool canUseMsi =
                !string.Equals(config["WEBSITE_DISABLE_MSI"], bool.TrueString, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(config["MSI_ENDPOINT"]) &&
                !string.IsNullOrEmpty(config["MSI_SECRET"]);

            bool canUseKeyVault =
                !string.IsNullOrEmpty(vault) &&
                (canUseMsi || (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret)));

            if (canUseKeyVault)
            {
                AzureKeyVaultConfigurationOptions options;

                if (canUseMsi)
                {
                    options = new AzureKeyVaultConfigurationOptions(vault);
                }
                else
                {
                    options = new AzureKeyVaultConfigurationOptions(vault, clientId, clientSecret);
                }

                options.Manager = new AzureEnvironmentSecretManager(config.AzureEnvironment());

                builder.AddAzureKeyVault(options);
            }

            return builder;
        }
    }
}
