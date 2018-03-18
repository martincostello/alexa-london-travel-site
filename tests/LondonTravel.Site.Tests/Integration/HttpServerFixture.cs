// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;

    /// <summary>
    /// A test fixture representing an HTTP server hosting the website.
    /// </summary>
    public class HttpServerFixture : WebApplicationTestFixture<TestStartup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
            : base("src/LondonTravel.Site")
        {
            Client.BaseAddress = new System.Uri("https://localhost");
        }

        /// <summary>
        /// Gets the <see cref="TestServer"/> in use.
        /// </summary>
        public TestServer Server { get; private set; }

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
        }

        /// <inheritdoc />
        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            return Server = base.CreateServer(builder);
        }
    }
}
