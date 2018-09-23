// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Xunit;

    /// <summary>
    /// A class representing the collection fixture for a test server. This class cannot be inherited.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)] // Causes issues with ApplicationInsights if not disabled
    public sealed class TestServerCollection : ICollectionFixture<TestServerFixture>
    {
        /// <summary>
        /// The name of the test fixture.
        /// </summary>
        public const string Name = "Test server collection";
    }
}
