// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Extensions;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A class representing an Application Insights initializer for custom telemetry setup. This class cannot be inherited.
    /// </summary>
    internal sealed class SiteTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// The <see cref="IConfiguration"/> to use. This field is read-only.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The <see cref="IHttpContextAccessor"/> to use. This field is read-only.
        /// </summary>
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteTelemetryInitializer"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
        public SiteTelemetryInitializer(IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc />
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["AzureDatacenter"] = _config.AzureDatacenter();
            telemetry.Context.GlobalProperties["AzureEnvironment"] = _config.AzureEnvironment();
            telemetry.Context.GlobalProperties["GitCommit"] = GitMetadata.Commit;

            if (_contextAccessor.HttpContext != null && string.IsNullOrEmpty(telemetry.Context.User.AuthenticatedUserId))
            {
                if (_contextAccessor.HttpContext.User?.Identity?.IsAuthenticated == true)
                {
                    telemetry.Context.User.AuthenticatedUserId = _contextAccessor.HttpContext.User.GetUserId();
                }
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
}
