// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
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

        services
            .AddOpenTelemetry()
            .WithMetrics((builder) =>
            {
                builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddProcessInstrumentation()
                       .AddMeter("System.Runtime");

                if (ApplicationTelemetry.IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            })
            .WithTracing((builder) =>
            {
                builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddSource(ApplicationTelemetry.ServiceName)
                       .AddSource("Azure.*");

                if (environment.IsDevelopment())
                {
                    builder.SetSampler(new AlwaysOnSampler());
                }

                if (ApplicationTelemetry.IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure<IServiceProvider>((options, provider) =>
                {
                    AddServiceMappings(ServiceMap, provider);

                    options.EnrichWithHttpRequestMessage = EnrichHttpActivity;
                    options.EnrichWithHttpResponseMessage = EnrichHttpActivity;

                    options.RecordException = true;
                });
    }

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
