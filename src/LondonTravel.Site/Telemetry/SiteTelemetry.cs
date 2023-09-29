// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.ApplicationInsights;

namespace MartinCostello.LondonTravel.Site.Telemetry;

/// <summary>
/// A class representing the default implementation of <see cref="ISiteTelemetry"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SiteTelemetry"/> class.
/// </remarks>
/// <param name="client">The <see cref="TelemetryClient"/> to use.</param>
internal sealed class SiteTelemetry(TelemetryClient client) : ISiteTelemetry
{
    /// <inheritdoc />
    public void TrackEvent(string eventName, IDictionary<string, string?>? properties = null)
    {
        client.TrackEvent(eventName, properties);
    }
}
