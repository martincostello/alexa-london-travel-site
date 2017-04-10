// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the interface for recording telemetry.
    /// </summary>
    public interface ISiteTelemetry
    {
        /// <summary>
        /// Send information about external dependency call in the application as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the result of the external dependency.</typeparam>
        /// <param name="dependencyName">External dependency name.</param>
        /// <param name="commandName">Dependency call command name.</param>
        /// <param name="operation">A delegate to a method that represents the external dependency call.</param>
        /// <param name="wasSuccessful">An optional delegate to a method that determines whether the result is successful.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the result from invoking <paramref name="operation"/>.
        /// </returns>
        Task<T> TrackDependencyAsync<T>(
            string dependencyName,
            string commandName,
            Func<Task<T>> operation,
            Predicate<T> wasSuccessful = null);

        /// <summary>
        /// Tracks an event.
        /// </summary>
        /// <param name="eventName">The name of the event to track.</param>
        /// <param name="properties">The optional properties associated with the event.</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null);
    }
}
