// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using Extensions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup : StartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IHostingEnvironment env)
            : base(env)
        {
            Configuration = BuildConfiguration(env);
            ConfigureSerilog(env, Configuration);
        }

        /// <summary>
        /// Builds the <see cref="IConfigurationRoot"/> to use for the application.
        /// </summary>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> to use.</param>
        /// <returns>
        /// The <see cref="IConfigurationRoot"/> to use.
        /// </returns>
        private static IConfigurationRoot BuildConfiguration(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            bool isDevelopment = environment.IsDevelopment();

            if (isDevelopment)
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddApplicationInsightsSettings(developerMode: isDevelopment);

            return TryConfigureAzureKeyVault(builder);
        }

        /// <summary>
        /// Tries to configure Azure Key Vault.
        /// </summary>
        /// <param name="builder">The current <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>
        /// The <see cref="IConfigurationRoot"/> to use for the application.
        /// </returns>
        private static IConfigurationRoot TryConfigureAzureKeyVault(IConfigurationBuilder builder)
        {
            // Build the main configuration
            IConfigurationRoot config = builder.Build();

            // Get the settings for Azure Key Vault
            string vault = config["AzureKeyVault:Uri"];
            string msiEndpoint = config["MSI_ENDPOINT"];
            string msiSecret = config["MSI_SECRET"];

            bool canUseKeyVault =
                !string.IsNullOrEmpty(vault) &&
                !string.IsNullOrEmpty(msiEndpoint) &&
                !string.IsNullOrEmpty(msiSecret);

            if (canUseKeyVault)
            {
                // Add Azure Key Vault and replace the configuration built already
                var manager = new AzureEnvironmentSecretManager(config.AzureEnvironment());

                var provider = new AzureServiceTokenProvider();
                var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(provider.KeyVaultTokenCallback));
                builder.AddAzureKeyVault(vault, client, manager);

                config = builder.Build();
            }

            return config;
        }

        /// <summary>
        /// Configures Serilog for the application.
        /// </summary>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> to use.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        private static void ConfigureSerilog(IHostingEnvironment environment, IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("AspNetCoreEnvironment", environment.EnvironmentName)
                .Enrich.WithProperty("AzureDatacenter", configuration.AzureDatacenter())
                .Enrich.WithProperty("AzureEnvironment", configuration.AzureEnvironment())
                .Enrich.WithProperty("Version", GitMetadata.Commit)
                .ReadFrom.Configuration(configuration)
                .WriteTo.ApplicationInsightsEvents(configuration.ApplicationInsightsKey());

            if (environment.IsDevelopment())
            {
                loggerConfig = loggerConfig.WriteTo.LiterateConsole();
            }

            string papertrailHostname = configuration.PapertrailHostname();

            if (!string.IsNullOrWhiteSpace(papertrailHostname))
            {
                loggerConfig.WriteTo.Papertrail(papertrailHostname, configuration.PapertrailPort());
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// A class representing an implementation of <see cref="IKeyVaultSecretManager"/>
        /// that selects keys based on the Azure environment name.
        /// </summary>
        private sealed class AzureEnvironmentSecretManager : IKeyVaultSecretManager
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
            public string GetKey(SecretBundle secret)
            {
                return secret.SecretIdentifier.Name.Substring(_prefix.Length)
                    .Replace("--", "_")
                    .Replace("-", ":");
            }

            /// <inheritdoc />
            public bool Load(SecretItem secret)
            {
                return secret.Identifier.Name.StartsWith(_prefix);
            }
        }
    }
}
