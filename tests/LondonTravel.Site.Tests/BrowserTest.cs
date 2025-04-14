// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.LondonTravel.Site.Pages;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// The base class for browser tests.
/// </summary>
[Category("UI")]
public abstract class BrowserTest(ITestOutputHelper outputHelper) : IAsyncLifetime, IDisposable
{
    /// <summary>
    /// Finalizes an instance of the <see cref="BrowserTest"/> class.
    /// </summary>
    ~BrowserTest()
    {
        Dispose(false);
    }

    protected bool CaptureTrace { get; set; } = IsRunningInGitHubActions;

    protected bool CaptureVideo { get; set; } = IsRunningInGitHubActions;

    /// <summary>
    /// Gets the <see cref="ITestOutputHelper"/> to use.
    /// </summary>
    protected ITestOutputHelper Output { get; } = outputHelper;

    /// <summary>
    /// Gets the URI of the website being tested.
    /// </summary>
    protected abstract Uri ServerAddress { get; }

    private static bool IsRunningInGitHubActions { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public ValueTask InitializeAsync()
    {
        InstallPlaywright();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> as an asynchronous operation.
    /// </summary>
    /// <param name="browserType">The type of the browser to run the test with.</param>
    /// <param name="browserChannel">The optional browser channel to use.</param>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task WithNavigatorAsync(
        string browserType,
        string? browserChannel,
        Func<ApplicationNavigator, Task> test,
        [CallerMemberName] string? testName = null)
    {
        var options = new BrowserFixtureOptions()
        {
            BrowserChannel = browserChannel,
            BrowserType = browserType,
            CaptureTrace = CaptureTrace,
            CaptureVideo = CaptureVideo,
        };

        var fixture = new BrowserFixture(options, Output);
        await fixture.WithPageAsync(
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
    /// <param name="browserChannel">The optional browser channel to use.</param>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task AtPageAsync<T>(
        string browserType,
        string? browserChannel,
        Func<ApplicationNavigator, T, Task> test,
        [CallerMemberName] string? testName = null)
        where T : PageBase
    {
        await WithNavigatorAsync(
            browserType,
            browserChannel,
            async (navigator) =>
            {
                var page = Activator.CreateInstance(typeof(T), navigator) as T;
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
    /// <param name="browserChannel">The optional browser channel to use.</param>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task AtPageAsync<T>(
        string browserType,
        string? browserChannel,
        Func<T, Task> test,
        [CallerMemberName] string? testName = null)
        where T : PageBase
    {
        await AtPageAsync<T>(
            browserType,
            browserChannel,
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

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
