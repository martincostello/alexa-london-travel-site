// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;

    internal static class IServiceCollectionExtensions
    {
        internal static void DisableApplicationInsights(this IServiceCollection services)
        {
            // Disable dependency tracking to work around https://github.com/Microsoft/ApplicationInsights-dotnet-server/pull/1006
            services.Configure<TelemetryConfiguration>((p) => p.DisableTelemetry = true);

            // HACK Very hackily neutralise these telemetry modules. They always subscribe,
            // diagnostic listeners even if Application Insights is turned off, and that's
            // where the bug is. This prematurely initializes them and then disposes of them
            // so that they remove their diagnostic listeners that are wired up that causes the problems.
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, _) =>
                {
                    module.DisableDiagnosticSourceInstrumentation = true;
                    module.DisableRuntimeInstrumentation = true;
                    module.SetComponentCorrelationHttpHeaders = false;
                    module.IncludeDiagnosticSourceActivities.Clear();

                    module.Initialize(TelemetryConfiguration.Active);
                    module.Dispose();
                });

            services.ConfigureTelemetryModule<RequestTrackingTelemetryModule>(
                (module, _) =>
                {
                    module.CollectionOptions = new RequestCollectionOptions();
                    module.Initialize(TelemetryConfiguration.Active);
                    module.Dispose();
                });
        }
    }
}
