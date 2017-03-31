// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
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
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            bool isDevelopment = env.IsDevelopment();

            if (isDevelopment)
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddApplicationInsightsSettings(developerMode: isDevelopment);

            Configuration = builder.Build();

            ConfigureSerilog(env, Configuration);
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
                .Enrich.WithProperty("AzureDatacenter", configuration["Azure:Datacenter"] ?? "local")
                .Enrich.WithProperty("AzureEnvironment", configuration["Azure:Environment"] ?? "local")
                .Enrich.WithProperty("Version", GitMetadata.Commit)
                .ReadFrom.Configuration(configuration)
                .WriteTo.ApplicationInsightsEvents(configuration["ApplicationInsights:InstrumentationKey"]);

            if (environment.IsDevelopment())
            {
                loggerConfig = loggerConfig.WriteTo.LiterateConsole();
            }

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
