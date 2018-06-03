// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// A class representing a filter for HTTP requests to Application Insights. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Based on <c>https://github.com/Microsoft/ApplicationInsights-dotnet-server/blob/db53106c5d85546a2508ecb9dcd6bf1106fb29fa/Src/DependencyCollector/Shared/Implementation/ApplicationInsightsUrlFilter.cs</c>.
    /// </remarks>
    internal sealed class ApplicationInsightsUrlFilter
    {
        private const string QuickPulseServiceEndpoint = "https://rt.services.visualstudio.com/QuickPulseService.svc";
        private const string TelemetryServiceEndpoint = "https://dc.services.visualstudio.com/v2/track";

        private readonly TelemetryConfiguration _configuration;
        private readonly Uri _telemetryServiceEndpointUri = new Uri(TelemetryServiceEndpoint);

        private KeyValuePair<string, string> _cachedEndpointLeftPart;

        internal ApplicationInsightsUrlFilter(TelemetryConfiguration configuration)
        {
            _configuration = configuration;
            _cachedEndpointLeftPart = GetEndpointLeftPart();
        }

        private string EndpointLeftPart
        {
            get
            {
                string currentEndpointAddressValue = null;

                if (_configuration != null)
                {
                    string endpoint = _configuration.TelemetryChannel?.EndpointAddress;

                    if (!string.IsNullOrEmpty(endpoint))
                    {
                        currentEndpointAddressValue = endpoint;
                    }
                }

                if (_cachedEndpointLeftPart.Key != currentEndpointAddressValue)
                {
                    _cachedEndpointLeftPart = GetEndpointLeftPart(currentEndpointAddressValue);
                }

                return _cachedEndpointLeftPart.Value;
            }
        }

        internal bool IsApplicationInsightsUrl(Uri uri)
        {
            if (SdkInternalOperationsMonitor.IsEntered())
            {
                return true;
            }

            bool result = false;
            string url = uri?.ToString();

            if (!string.IsNullOrEmpty(url))
            {
                result = url.StartsWith(TelemetryServiceEndpoint, StringComparison.OrdinalIgnoreCase) ||
                         url.StartsWith(QuickPulseServiceEndpoint, StringComparison.OrdinalIgnoreCase);

                if (!result)
                {
                    string endpointUrl = EndpointLeftPart;

                    if (!string.IsNullOrEmpty(endpointUrl))
                    {
                        result = url.StartsWith(endpointUrl, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            return result;
        }

        private KeyValuePair<string, string> GetEndpointLeftPart(string currentEndpointAddressValue = null)
        {
            Uri uri = currentEndpointAddressValue != null ? new Uri(currentEndpointAddressValue) : _telemetryServiceEndpointUri;
            string endpointLeftPart = uri.Scheme + "://" + uri.Authority;

            return new KeyValuePair<string, string>(currentEndpointAddressValue, endpointLeftPart);
        }
    }
}
