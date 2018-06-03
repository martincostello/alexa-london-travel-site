// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using JustEat.HttpClientInterception;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Pages;
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
        /// Creates a new instance of <see cref="ApplicationNavigator"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="ApplicationNavigator"/> to use for tests.
        /// </returns>
        protected ApplicationNavigator CreateNavigator() => new ApplicationNavigator(Fixture.ServerAddress, CreateWebDriver());

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

            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

#if DEBUG
            options.SetLoggingPreference(LogType.Client, LogLevel.All);
            options.SetLoggingPreference(LogType.Driver, LogLevel.All);
            options.SetLoggingPreference(LogType.Profiler, LogLevel.All);
            options.SetLoggingPreference(LogType.Server, LogLevel.All);
#endif

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
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/>.
        /// </summary>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void WithNavigator(Action<ApplicationNavigator> test, [CallerMemberName] string testName = null)
        {
            using (ApplicationNavigator navigator = CreateNavigator())
            {
                try
                {
                    test(navigator);
                }
                catch (Exception)
                {
                    TakeScreenshot(navigator.Driver, testName);
                    throw;
                }
                finally
                {
                    OutputLogs(navigator.Driver);
                }
            }
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

        private void OutputLogs(IWebDriver driver)
        {
            try
            {
                var logs = driver.Manage().Logs;

                var allEntries = new List<Tuple<string, LogEntry>>();

                foreach (string logKind in logs.AvailableLogTypes)
                {
                    var logEntries = logs.GetLog(logKind)
                        .Select((p) => Tuple.Create(logKind, p))
                        .ToList();

                    allEntries.AddRange(logEntries);
                }

                foreach (var logEntry in allEntries.OrderBy((p) => p.Item2.Timestamp))
                {
                    var logKind = logEntry.Item1;
                    var entry = logEntry.Item2;
                    Output.WriteLine($"[{entry.Timestamp:u}] {logKind} - {entry.Level}: {entry.Message}");
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Failed to output driver logs: {ex.ToString()}");
            }
        }

        private void TakeScreenshot(IWebDriver driver, string testName)
        {
            try
            {
                if (driver is ITakesScreenshot camera)
                {
                    Screenshot screenshot = camera.GetScreenshot();

                    string directory = Path.GetDirectoryName(typeof(BrowserTest).Assembly.Location);
                    string fileName = $"{testName}_{DateTimeOffset.UtcNow:YYYY-MM-dd-HH-mm-ss}.png";

                    fileName = Path.Combine(directory, fileName);

                    screenshot.SaveAsFile(fileName);
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Failed to take screenshot: {ex.ToString()}");
            }
        }
    }
}
