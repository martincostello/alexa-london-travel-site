// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger
{
    /// <summary>
    /// Defines a method for obtaining examples for Swagger documentation.
    /// </summary>
    internal interface IExampleProvider
    {
        /// <summary>
        /// Gets the example to use.
        /// </summary>
        /// <returns>
        /// An <see cref="object"/> that should be used as the example.
        /// </returns>
        object GetExample();
    }
}
