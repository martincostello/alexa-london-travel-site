// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
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

        /// <summary>
        /// Creates a new instance of <see cref="IWebDriver"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="IWebDriver"/> to use for tests.
        /// </returns>
        protected IWebDriver CreateWebDriver()
        {
            string chromeDriverDirectory = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
            };

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                options.AddArgument("--headless");
            }

            return new ChromeDriver(chromeDriverDirectory, options);
        }
    }
}
