// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Azure;
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MartinCostello.LondonTravel.Site.Extensions;

public static class TelemetryExtensions
{
    private static readonly ConcurrentDictionary<string, string> ServiceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["api.github.com"] = "GitHub",
    };

    public static void AddTelemetry(this IServiceCollection services, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ApplicationTelemetry.ServiceName, serviceVersion: ApplicationTelemetry.ServiceVersion)
            .AddDetector(new AppServiceResourceDetector())
            .AddDetector(new ContainerResourceDetector());

        if (IsAzureMonitorConfigured())
        {
            services.Configure<AzureMonitorExporterOptions>(
                (p) => p.ConnectionString = AzureMonitorConnectionString());
        }

        services
            .AddOpenTelemetry()
            .WithMetrics((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();

                if (IsAzureMonitorConfigured())
                {
                    builder.AddAzureMonitorMetricExporter();
                }

                if (IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            })
            .WithTracing((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddSource(ApplicationTelemetry.ServiceName);

                if (environment.IsDevelopment())
                {
                    builder.SetSampler(new AlwaysOnSampler());
                }

                if (IsAzureMonitorConfigured())
                {
                    builder.AddAzureMonitorTraceExporter();
                }

                if (IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    AddServiceMappings(ServiceMap, provider);

                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.RecordException = true;
                });
    }

    internal static bool IsOtlpCollectorConfigured()
        => !string.IsNullOrEmpty(AzureMonitorConnectionString());

    internal static bool IsAzureMonitorConfigured()
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"));

    private static string? AzureMonitorConnectionString()
        => Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

    private static void EnrichHttpActivity(Activity activity, HttpRequestMessage request)
    {
        TryEnrichWithPeerService(activity);

        static void TryEnrichWithPeerService(Activity activity)
        {
            if (GetTag("server.address", activity.Tags) is { Length: > 0 } hostName)
            {
                if (!ServiceMap.TryGetValue(hostName, out string? service))
                {
                    service = hostName;
                }

                activity.AddTag("peer.service", service);
            }
        }

        static string? GetTag(string name, IEnumerable<KeyValuePair<string, string?>> tags)
            => tags.FirstOrDefault((p) => p.Key == name).Value;
    }

    private static void AddServiceMappings(ConcurrentDictionary<string, string> mappings, IServiceProvider serviceProvider)
    {
        AddMappings<AmazonAuthenticationOptions>("Amazon");
        AddMappings<AppleAuthenticationOptions>("Apple");
        AddMappings<GitHubAuthenticationOptions>("GitHub");
        AddMappings<GoogleOptions>("Google");
        AddMappings<MicrosoftAccountOptions>("Microsoft");
        AddMapping("Twitter", "https://api.twitter.com");

        void AddMapping(string name, string? host)
        {
            if (host is { Length: > 0 } url &&
                Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                !mappings.ContainsKey(uri.Host))
            {
                mappings[uri.Host] = name;
            }
        }

        void AddMappings<T>(string name)
            where T : OAuthOptions
        {
            var options = GetOAuthOptions<T>();

            if (options is { })
            {
                AddMapping(name, options.AuthorizationEndpoint);
                AddMapping(name, options.TokenEndpoint);
                AddMapping(name, options.UserInformationEndpoint);
            }
        }

        OAuthOptions? GetOAuthOptions<T>()
            where T : OAuthOptions
            => serviceProvider.GetService<IOptions<T>>()?.Value;
    }
}
