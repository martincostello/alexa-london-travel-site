// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
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
            telemetry.Context.Properties["AzureDatacenter"] = _config.AzureDatacenter();
            telemetry.Context.Properties["AzureEnvironment"] = _config.AzureEnvironment();
            telemetry.Context.Properties["GitCommit"] = GitMetadata.Commit;

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
            }
        }
    }
}
