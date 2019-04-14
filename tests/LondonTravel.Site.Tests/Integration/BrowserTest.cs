// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using JustEat.HttpClientInterception;
    using Microsoft.Extensions.Logging;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using Pages;
    using Xunit;
    using Xunit.Abstractions;
    using LogLevel = OpenQA.Selenium.LogLevel;

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
            Fixture.SetOutputHelper(outputHelper);
            Output = outputHelper;

            var logger = outputHelper.ToLogger<HttpClientInterceptorOptions>();

            Fixture.Interceptor.OnSend = (request) =>
            {
                logger.LogInformation("HTTP request intercepted. {Request}", request);
                return Task.CompletedTask;
            };

            _scope = Fixture.Interceptor.BeginScope();
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
        /// <param name="collectPerformanceLogs">Whether to collect browser performance logs.</param>
        /// <returns>
        /// The <see cref="ApplicationNavigator"/> to use for tests.
        /// </returns>
        protected ApplicationNavigator CreateNavigator(bool collectPerformanceLogs = false)
            => new ApplicationNavigator(Fixture.ServerAddress, CreateWebDriver(collectPerformanceLogs));

        /// <summary>
        /// Creates a new instance of <see cref="IWebDriver"/>.
        /// </summary>
        /// <param name="collectPerformanceLogs">Whether to collect browser performance logs.</param>
        /// <returns>
        /// The <see cref="IWebDriver"/> to use for tests.
        /// </returns>
        protected IWebDriver CreateWebDriver(bool collectPerformanceLogs = false)
        {
            string chromeDriverDirectory = Path.GetDirectoryName(GetType().Assembly.Location);

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
            };

            bool isDebuggerAttached = System.Diagnostics.Debugger.IsAttached;

            if (!isDebuggerAttached)
            {
                options.AddArgument("--headless");
            }

            // HACK Workaround for "(unknown error: DevToolsActivePort file doesn't exist)"
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) &&
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.AddArgument("--no-sandbox");
            }

            if (collectPerformanceLogs || isDebuggerAttached)
            {
                // Enable logging of redirects (see https://stackoverflow.com/a/42212131/1064169)
                options.PerformanceLoggingPreferences = new ChromePerformanceLoggingPreferences();
                options.PerformanceLoggingPreferences.AddTracingCategory("devtools.network");
                options.AddAdditionalCapability(CapabilityType.EnableProfiling, true, true);
                options.SetLoggingPreference("performance", LogLevel.All);
            }

            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

#if DEBUG
            options.SetLoggingPreference(LogType.Client, LogLevel.All);
            options.SetLoggingPreference(LogType.Profiler, LogLevel.All);
            options.SetLoggingPreference(LogType.Server, LogLevel.All);
#endif

            var timeout = TimeSpan.FromSeconds(10);
            var driver = new ChromeDriver(chromeDriverDirectory, options, timeout);

            try
            {
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
                    OutputLogs(navigator.Driver);
                    TakeScreenshot(navigator.Driver, testName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> as an asynchronous operation.
        /// </summary>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="collectPerformanceLogs">Whether to collect browser performance logs.</param>
        /// <param name="testName">The name of the test method.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the test.
        /// </returns>
        protected async Task WithNavigatorAsync(
            Func<ApplicationNavigator, Task> test,
            bool collectPerformanceLogs = false,
            [CallerMemberName] string testName = null)
        {
            using (ApplicationNavigator navigator = CreateNavigator(collectPerformanceLogs))
            {
                try
                {
                    await test(navigator);
                }
                catch (Exception)
                {
                    OutputLogs(navigator.Driver);
                    TakeScreenshot(navigator.Driver, testName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void AtPage<T>(Action<ApplicationNavigator, T> test, [CallerMemberName] string testName = null)
            where T : PageBase
        {
            WithNavigator(
                (navigator) =>
                {
                    T page = ((T)Activator.CreateInstance(typeof(T), navigator)).Navigate();
                    test(navigator, page);
                },
                testName: testName);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void AtPage<T>(Action<T> test, [CallerMemberName] string testName = null)
            where T : PageBase
        {
            AtPage<T>((_, page) => test(page), testName: testName);
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
                    Fixture?.ClearOutputHelper();
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
                    directory = Path.Combine(directory, "screenshots");

                    string fileName = $"{testName}_{DateTimeOffset.UtcNow:yyyy-MM-dd-HH-mm-ss}.png";
                    fileName = Path.Combine(directory, fileName);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

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
