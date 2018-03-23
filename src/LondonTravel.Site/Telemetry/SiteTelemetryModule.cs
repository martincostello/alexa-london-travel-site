// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// A class representing an Application Insights module for custom telemetry. This class cannot be inherited.
    /// </summary>
    internal sealed class SiteTelemetryModule : ITelemetryModule, IDisposable
    {
        /// <summary>
        /// The object used for synchronization. This field is read-only.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// The activity enricher in use for HTTP calls.
        /// </summary>
        private HttpRequestActivityEnricher _enricher;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Whether the instance has been initialized.
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Finalizes an instance of the <see cref="SiteTelemetryModule"/> class.
        /// </summary>
        ~SiteTelemetryModule()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize(TelemetryConfiguration configuration)
        {
            if (!_initialized)
            {
                lock (_syncRoot)
                {
                    if (!_initialized)
                    {
                        _enricher = new HttpRequestActivityEnricher(configuration);
                        _enricher.Subscribe();

                        _initialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _enricher?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
