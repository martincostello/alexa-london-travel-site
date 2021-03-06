// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MartinCostello.LondonTravel.Site.Pages;
using Xunit.Abstractions;

namespace MartinCostello.LondonTravel.Site
{
    /// <summary>
    /// The base class for browser tests.
    /// </summary>
    public abstract class BrowserTest : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        protected BrowserTest(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BrowserTest"/> class.
        /// </summary>
        ~BrowserTest()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="ITestOutputHelper"/> to use.
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// Gets the URI of the website being tested.
        /// </summary>
        protected abstract Uri ServerAddress { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> as an asynchronous operation.
        /// </summary>
        /// <param name="browserType">The type of the browser to run the test with.</param>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the test.
        /// </returns>
        protected async Task WithNavigatorAsync(
            string browserType,
            Func<ApplicationNavigator, Task> test,
            [CallerMemberName] string? testName = null)
        {
            var fixture = new BrowserFixture(Output);

            await fixture.WithPageAsync(
                browserType,
                async (page) =>
                {
                    var navigator = new ApplicationNavigator(ServerAddress, page);
                    await test(navigator);
                },
                testName);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="browserType">The type of the browser to run the test with.</param>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the test.
        /// </returns>
        protected async Task AtPageAsync<T>(
            string browserType,
            Func<ApplicationNavigator, T, Task> test,
            [CallerMemberName] string? testName = null)
            where T : PageBase
        {
            await WithNavigatorAsync(
                browserType,
                async (navigator) =>
                {
                    T? page = Activator.CreateInstance(typeof(T), navigator) as T;
                    await page!.NavigateAsync();

                    await test(navigator, page!);
                },
                testName: testName);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="browserType">The type of the browser to run the test with.</param>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the test.
        /// </returns>
        protected async Task AtPageAsync<T>(
            string browserType,
            Func<T, Task> test,
            [CallerMemberName] string? testName = null)
            where T : PageBase
        {
            await AtPageAsync<T>(
                browserType,
                async (_, page) => await test(page),
                testName: testName);
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
            // No-op
        }
    }
}
