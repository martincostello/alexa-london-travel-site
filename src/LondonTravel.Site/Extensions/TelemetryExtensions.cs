// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.Tracing;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Site.Extensions;

public static class TelemetryExtensions
{
    public static void AddTelemetry(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddMetrics();
        builder.Services.AddTracing((p) => p.AddConfiguration(builder.Configuration.GetSection("Tracing")));

        var telemetry = builder.Services.AddOpenTelemetry();

        if (ApplicationTelemetry.IsOtlpCollectorConfigured())
        {
            telemetry.UseOtlpExporter();
        }

        telemetry.WithMetrics((metrics) =>
               {
                   metrics.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                          .AddAspNetCoreInstrumentation()
                          .AddHttpClientInstrumentation()
                          .AddProcessInstrumentation()
                          .SetExemplarFilter(ExemplarFilterType.TraceBased);
               })
               .WithTracing((traces) =>
               {
                   traces.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                         .AddAspNetCoreInstrumentation()
                         .AddHttpClientInstrumentation();

                   if (builder.Environment.IsDevelopment())
                   {
                       traces.SetSampler(new AlwaysOnSampler());
                   }
               });

        builder.Services
            .AddOptions<HttpClientTraceInstrumentationOptions>()
            .Configure((options) =>
            {
                options.EnrichWithHttpResponseMessage = EnrichHttpActivity;
                options.RecordException = true;
            });

        builder.Logging.AddTelemetry();
    }

    private static void EnrichHttpActivity(Activity activity, HttpResponseMessage response)
    {
        if (response.RequestMessage?.Headers.TryGetValues("x-ms-client-request-id", out var clientRequestId) is true)
        {
            activity.SetTag("az.client_request_id", clientRequestId);
        }

        if (response.Headers.TryGetValues("x-ms-request-id", out var requestId))
        {
            activity.SetTag("az.service_request_id", requestId);
        }
    }
}
