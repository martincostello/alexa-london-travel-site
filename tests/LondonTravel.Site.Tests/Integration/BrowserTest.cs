// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Xunit;

    /// <summary>
    /// The base class for browser tests.
    /// </summary>
    [Collection(HttpServerCollection.Name)]
    public abstract class BrowserTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        protected BrowserTest(HttpServerFixture fixture)
        {
            Fixture = fixture;
        }

        /// <summary>
        /// Gets the <see cref="HttpServerFixture"/> to use.
        /// </summary>
        protected HttpServerFixture Fixture { get; }
    }
}
