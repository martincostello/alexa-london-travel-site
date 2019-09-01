// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// A class representing the default implementation of <see cref="ISiteTelemetry"/>. This class cannot be inherited.
    /// </summary>
    internal sealed class SiteTelemetry : ISiteTelemetry
    {
        /// <summary>
        /// The <see cref="TelemetryClient"/> to use. This field is read-only.
        /// </summary>
        private readonly TelemetryClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteTelemetry"/> class.
        /// </summary>
        /// <param name="client">The <see cref="TelemetryClient"/> to use.</param>
        public SiteTelemetry(TelemetryClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
        {
            _client.TrackEvent(eventName, properties);
        }
    }
}
