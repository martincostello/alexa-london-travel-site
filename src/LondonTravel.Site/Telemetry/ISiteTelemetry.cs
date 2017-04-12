// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the interface for recording telemetry.
    /// </summary>
    public interface ISiteTelemetry
    {
        /// <summary>
        /// Tracks an event.
        /// </summary>
        /// <param name="eventName">The name of the event to track.</param>
        /// <param name="properties">The optional properties associated with the event.</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null);
    }
}
