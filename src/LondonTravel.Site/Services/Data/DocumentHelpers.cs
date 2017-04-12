// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Options;

    /// <summary>
    /// A class containing helper methods for DocumentDB operations. This class cannot be inherited.
    /// </summary>
    internal static class DocumentHelpers
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DocumentClient"/> class.
        /// </summary>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="DocumentClient"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        internal static DocumentClient CreateClient(UserStoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ServiceUri == null)
            {
                throw new ArgumentException("No DocumentDB URI is configured.", nameof(options));
            }

            if (!options.ServiceUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The configured DocumentDB URI is as it is not an absolute URI.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.AccessKey))
            {
                throw new ArgumentException("No DocumentDB access key is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.DatabaseName))
            {
                throw new ArgumentException("No DocumentDB database name is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.CollectionName))
            {
                throw new ArgumentException("No DocumentDB collection name is configured.", nameof(options));
            }

            ConnectionPolicy connectionPolicy = null;

            if (options.PreferredLocations?.Count > 0)
            {
                connectionPolicy = new ConnectionPolicy();

                foreach (string location in options.PreferredLocations)
                {
                    connectionPolicy.PreferredLocations.Add(location);
                }
            }

            return new DocumentClient(options.ServiceUri, options.AccessKey, connectionPolicy);
        }

        /// <summary>
        /// Tracks the specified DocumentDB request as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the response.</typeparam>
        /// <param name="client">The <see cref="TelemetryClient"/> to use to track the request.</param>
        /// <param name="serviceEndpoint">The URI of the DocumentDB service endpoint.</param>
        /// <param name="method">The HTTP method associated with the request.</param>
        /// <param name="relativeUri">The relative URI associated with the request.</param>
        /// <param name="request">A delegate to a method representing the request.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation which
        /// returns an instance of <typeparamref name="T"/> representing the result of the request.
        /// </returns>
        internal static async Task<T> TrackAsync<T>(TelemetryClient client, Uri serviceEndpoint, HttpMethod method, Uri relativeUri, Func<Task<T>> request)
            where T : IResourceResponseBase
        {
            var httpMethod = method.Method;
            var requestUri = new Uri(serviceEndpoint, relativeUri);
            var resourceName = requestUri.AbsolutePath;

            resourceName = $"{httpMethod} {resourceName}";

            var telemetry = new DependencyTelemetry();

            client.Initialize(telemetry);

            telemetry.Data = requestUri.OriginalString;
            telemetry.Name = resourceName;
            telemetry.Target = requestUri.Host;
            telemetry.Type = "Http";

            telemetry.Properties["httpMethod"] = httpMethod;

            T result;
            int statusCode = 0;

            telemetry.Start();

            try
            {
                result = await request();
                statusCode = (int)result.StatusCode;
            }
            catch (DocumentClientException ex)
            {
                statusCode = (int)ex.StatusCode;
                throw;
            }
            finally
            {
                telemetry.Stop();

                telemetry.ResultCode = statusCode > 0 ? statusCode.ToString(CultureInfo.InvariantCulture) : string.Empty;
                telemetry.Success = statusCode > 0 && statusCode < 400;

                client.TrackDependency(telemetry);
            }

            return result;
        }
    }
}
