// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Threading.Tasks;
    using JustEat.HttpClientInterception;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// The base class for browser tests.
    /// </summary>
    [Collection(HttpServerCollection.Name)]
    public abstract class BrowserTest : IDisposable
    {
        private bool _disposed;
        private IDisposable _scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        protected BrowserTest(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Output = outputHelper;

            Fixture.Interceptor.OnSend = (request) =>
            {
                Output.WriteLine($"[{DateTimeOffset.UtcNow:u}] {request.ToString()}");
                return Task.CompletedTask;
            };

            _scope = Fixture.Interceptor.BeginScope();

            new Builders.TflInterceptionBuilder().ForLines().RegisterWith(Fixture.Interceptor);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        ~BrowserTest()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="HttpServerFixture"/> to use.
        /// </summary>
        protected HttpServerFixture Fixture { get; }

        /// <summary>
        /// Gets the <see cref="ITestOutputHelper"/> to use.
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

            var driver = new ChromeDriver(chromeDriverDirectory, options);

            try
            {
                var timeout = TimeSpan.FromSeconds(10);
                var timeouts = driver.Manage().Timeouts();

                timeouts.AsynchronousJavaScript = timeout;
                timeouts.ImplicitWait = timeout;
                timeouts.PageLoad = timeout;
            }
            catch (Exception)
            {
                driver.Dispose();
                throw;
            }

            return driver;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _scope?.Dispose();
                    _scope = null;
                }

                _disposed = true;
            }
        }
    }
}
