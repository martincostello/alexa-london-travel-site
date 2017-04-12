// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System.Globalization;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// A class representing a delegating HTTP message handler that tracks telemetry. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Based on <c>https://github.com/Microsoft/ApplicationInsights-dotnet-server</c>, which does not support .NET Core.
    /// </remarks>
    public sealed class TelemetryHttpMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// The <see cref="TelemetryClient"/> to use. This field is read-only.
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="telemetryClient">The <see cref="TelemetryClient"/> to use.</param>
        public TelemetryHttpMessageHandler(TelemetryClient telemetryClient)
            : base(new HttpClientHandler())
        {
            _telemetryClient = telemetryClient;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var telemetry = CreateTelemetry(request);

            var response = await base.SendAsync(request, cancellationToken);

            TrackResponse(response, telemetry);

            return response;
        }

        /// <summary>
        /// Creates a <see cref="DependencyTelemetry"/> for the specified <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request to create the telemetry for.</param>
        /// <returns>
        /// The <see cref="DependencyTelemetry"/> to use for <paramref name="request"/>.
        /// </returns>
        private DependencyTelemetry CreateTelemetry(HttpRequestMessage request)
        {
            var httpMethod = request.Method.Method;
            var requestUri = request.RequestUri;
            var resourceName = requestUri.AbsolutePath;

            if (!string.IsNullOrEmpty(httpMethod))
            {
                resourceName = $"{httpMethod} {resourceName}";
            }

            var telemetry = new DependencyTelemetry();

            _telemetryClient.Initialize(telemetry);

            telemetry.Data = requestUri.OriginalString;
            telemetry.Name = resourceName;
            telemetry.Target = requestUri.Host;
            telemetry.Type = "Http";

            telemetry.Start();

            return telemetry;
        }

        /// <summary>
        /// Tracks the specified HTTP response for the specified telemetry.
        /// </summary>
        /// <param name="response">The HTTP response to track.</param>
        /// <param name="telemetry">The populated telemetry data.</param>
        private void TrackResponse(HttpResponseMessage response, DependencyTelemetry telemetry)
        {
            telemetry.Stop();

            int statusCode = (int)response.StatusCode;
            telemetry.ResultCode = statusCode > 0 ? statusCode.ToString(CultureInfo.InvariantCulture) : string.Empty;
            telemetry.Success = statusCode > 0 && statusCode < 400;

            _telemetryClient.Track(telemetry);
        }
    }
}
