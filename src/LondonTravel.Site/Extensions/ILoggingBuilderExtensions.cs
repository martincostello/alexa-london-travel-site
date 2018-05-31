// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;

    /// <summary>
    /// A class containing extension methods for the <see cref="ILoggingBuilder"/> interface. This class cannot be inherited.
    /// </summary>
    public static class ILoggingBuilderExtensions
    {
        /// <summary>
        /// Configures logging for the application.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
        /// <param name="context">The <see cref="WebHostBuilderContext"/> to use.</param>
        /// <returns>
        /// The <see cref="ILoggingBuilder"/> passed as the value of <paramref name="builder"/>.
        /// </returns>
        public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder builder, WebHostBuilderContext context)
        {
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("AspNetCoreEnvironment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("AzureDatacenter", context.Configuration.AzureDatacenter())
                .Enrich.WithProperty("AzureEnvironment", context.Configuration.AzureEnvironment())
                .Enrich.WithProperty("Version", GitMetadata.Commit)
                .ReadFrom.Configuration(context.Configuration)
                .WriteTo.ApplicationInsightsEvents(context.Configuration.ApplicationInsightsKey());

            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfig = loggerConfig.WriteTo.LiterateConsole();
            }

            string papertrailHostname = context.Configuration.PapertrailHostname();

            if (!string.IsNullOrWhiteSpace(papertrailHostname))
            {
                loggerConfig.WriteTo.Papertrail(papertrailHostname, context.Configuration.PapertrailPort());
            }

            Log.Logger = loggerConfig.CreateLogger();
            return builder.AddSerilog(dispose: true);
        }
    }
}
