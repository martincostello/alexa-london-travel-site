// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using OpenTelemetry.Logs;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="ILoggingBuilder"/> interface. This class cannot be inherited.
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// Adds OpenTelemetry logging.
    /// </summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <returns>
    /// The value specified by <paramref name="builder"/>.
    /// </returns>
    public static ILoggingBuilder AddTelemetry(this ILoggingBuilder builder)
    {
        return builder.AddOpenTelemetry((p) =>
        {
            p.IncludeFormattedMessage = true;
            p.IncludeScopes = true;

            if (TelemetryExtensions.IsOtlpCollectorConfigured())
            {
                p.AddOtlpExporter();
            }
        });
    }
}
