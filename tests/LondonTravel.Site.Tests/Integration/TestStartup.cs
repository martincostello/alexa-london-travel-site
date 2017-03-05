// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A class representing the startup logic for the tests.
    /// </summary>
    public class TestStartup : StartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestStartup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public TestStartup(IHostingEnvironment env)
            : base(env)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
