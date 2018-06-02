// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using Xunit;

    /// <summary>
    /// A test that does not run in a continuous integration build. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class NotCIFactAttribute : FactAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotCIFactAttribute"/> class.
        /// </summary>
        public NotCIFactAttribute()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            {
                Skip = "This test is not run as part of a continuous integration build.";
            }
        }
    }
}
