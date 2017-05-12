// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using Xunit;

    /// <summary>
    /// A test that requires a URI for Azure Cosmos DB to be configured as an environment variable. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class DocumentDbFactAttribute : FactAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbFactAttribute"/> class.
        /// </summary>
        public DocumentDbFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Site:Authentication:UserStore:ServiceUri")))
            {
                Skip = "No URI for Azure Cosmos DB is configured.";
            }
        }
    }
}
