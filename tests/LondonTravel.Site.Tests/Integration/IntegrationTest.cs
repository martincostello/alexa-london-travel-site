// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Xunit;

    /// <summary>
    /// The base class for integration tests.
    /// </summary>
    [Collection(TestServerCollection.Name)]
    public abstract class IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        protected IntegrationTest(TestServerFixture fixture)
        {
            Fixture = fixture;
        }

        /// <summary>
        /// Gets the <see cref="TestServerFixture"/> to use.
        /// </summary>
        protected TestServerFixture Fixture { get; }
    }
}
