// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A class containing extension methods for the <see cref="ISiteTelemetry"/> interface. This class cannot be inherited.
    /// </summary>
    public static class ISiteTelemetryExtensions
    {
        /// <summary>
        /// Send information about a DocumentDB call in the application as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the result of the external dependency.</typeparam>
        /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
        /// <param name="commandName">DocumentDB command name.</param>
        /// <param name="operation">A delegate to a method that represents the DocumentDB call.</param>
        /// <param name="wasSuccessful">An optional delegate to a method that determines whether the result is successful.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the result from invoking <paramref name="operation"/>.
        /// </returns>
        public static Task<T> TrackDocumentDbAsync<T>(
            this ISiteTelemetry telemetry,
            string commandName,
            Func<Task<T>> operation,
            Predicate<T> wasSuccessful = null)
        {
            return telemetry.TrackDependencyAsync("DocumentDB", commandName, operation, wasSuccessful);
        }
    }
}
