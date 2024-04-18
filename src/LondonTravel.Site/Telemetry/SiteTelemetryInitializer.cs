// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using MartinCostello.LondonTravel.Site.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace MartinCostello.LondonTravel.Site.Telemetry;

/// <summary>
/// A class representing an Application Insights initializer for custom telemetry setup. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SiteTelemetryInitializer"/> class.
/// </remarks>
/// <param name="config">The <see cref="IConfiguration"/> to use.</param>
/// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
internal sealed class SiteTelemetryInitializer(IConfiguration config, IHttpContextAccessor contextAccessor) : ITelemetryInitializer
{
    /// <inheritdoc />
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.GlobalProperties["AzureDatacenter"] = config.AzureDatacenter();
        telemetry.Context.GlobalProperties["AzureEnvironment"] = config.AzureEnvironment();
        telemetry.Context.GlobalProperties["GitCommit"] = GitMetadata.Commit;

        if (contextAccessor.HttpContext != null &&
            string.IsNullOrEmpty(telemetry.Context.User.AuthenticatedUserId) &&
            contextAccessor.HttpContext.User?.Identity?.IsAuthenticated is true)
        {
            telemetry.Context.User.AuthenticatedUserId = contextAccessor.HttpContext.User.GetUserId();
        }

        if (telemetry is DependencyTelemetry dependency)
        {
            var activity = System.Diagnostics.Activity.Current;

            if (activity != null)
            {
                foreach (var item in activity.Tags)
                {
                    dependency.Properties[item.Key] = item.Value;
                }
            }

            HttpResponseHeaders? headers = null;

            // See https://github.com/Microsoft/ApplicationInsights-dotnet-server/issues/587#issuecomment-443927313
            if (dependency.TryGetOperationDetail("HttpResponse", out object detail) && detail is HttpResponseMessage response)
            {
                headers = response.Headers;
            }
            else if (dependency.TryGetOperationDetail("HttpResponseHeaders", out detail) && detail is HttpResponseHeaders responseHeaders)
            {
                headers = responseHeaders;
            }

            if (headers != null && activity != null)
            {
                if (headers.TryGetValues("x-ms-activity-id", out var values))
                {
                    activity.AddTag("Activity Id", string.Join(", ", values));
                }

                if (headers.TryGetValues("x-ms-request-charge", out values))
                {
                    activity.AddTag("Request Charge", string.Join(", ", values));
                }
            }
        }
    }
}
