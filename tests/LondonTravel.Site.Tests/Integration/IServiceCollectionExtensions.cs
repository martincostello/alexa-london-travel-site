// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal static class IServiceCollectionExtensions
    {
        internal static void DisableApplicationInsights(this IServiceCollection services)
        {
            // Disable dependency tracking to work around https://github.com/Microsoft/ApplicationInsights-dotnet-server/pull/1006
            services.Configure<TelemetryConfiguration>((p) => p.DisableTelemetry = true);
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, _) =>
                {
                    module.DisableDiagnosticSourceInstrumentation = true;
                    module.DisableRuntimeInstrumentation = true;
                    module.SetComponentCorrelationHttpHeaders = false;
                    module.IncludeDiagnosticSourceActivities.Clear();
                });

            services.RemoveAll<ITelemetryInitializer>();
            services.RemoveAll<ITelemetryModule>();
        }
    }
}
