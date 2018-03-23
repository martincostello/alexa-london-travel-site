// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DiagnosticAdapter;

    /// <summary>
    /// A class that enriches the current activity with HTTP response headers. This class cannot be inherited.
    /// </summary>
    internal sealed class HttpRequestActivityEnricher : IObserver<DiagnosticListener>, IDisposable
    {
        /// <summary>
        /// The URL filter to use for Application Insights. This field is read-only.
        /// </summary>
        private readonly ApplicationInsightsUrlFilter _filter;

        /// <summary>
        /// The active diagnostic listener subscriptions. This field is read-only.
        /// </summary>
        private readonly IList<IDisposable> _subscriptions;

        public HttpRequestActivityEnricher(TelemetryConfiguration configuration)
        {
            _filter = new ApplicationInsightsUrlFilter(configuration);
            _subscriptions = new List<IDisposable>();
        }

        /// <summary>
        /// Subscribes the enricher to all diagnostic listeners.
        /// </summary>
        public void Subscribe()
        {
            _subscriptions.Add(DiagnosticListener.AllListeners.Subscribe(this));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }

        [DiagnosticName("System.Net.Http.HttpRequestOut.Stop")]
        public void OnHttpRequestOutStop(HttpRequestMessage request, HttpResponseMessage response, TaskStatus requestTaskStatus)
        {
            if (request != null && response != null && !_filter.IsApplicationInsightsUrl(request.RequestUri))
            {
                var activity = Activity.Current;

                if (activity != null)
                {
                    if (response.Headers.TryGetValues("x-ms-activity-id", out var values))
                    {
                        activity.AddTag("Activity Id", string.Join(", ", values));
                    }

                    if (response.Headers.TryGetValues("x-ms-request-charge", out values))
                    {
                        activity.AddTag("Request Charge", string.Join(", ", values));
                    }
                }
            }
        }

        /// <inheritdoc />
        void IObserver<DiagnosticListener>.OnCompleted()
        {
            // Not used
        }

        /// <inheritdoc />
        void IObserver<DiagnosticListener>.OnError(Exception error)
        {
            // Not used
        }

        /// <inheritdoc />
        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener value)
        {
            if (value.Name == "HttpHandlerDiagnosticListener")
            {
                _subscriptions.Add(value.SubscribeWithAdapter(this));
            }
        }
    }
}
