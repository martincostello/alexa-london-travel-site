// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using Xunit.Abstractions;

    public class BrowserFixture
    {
        public BrowserFixture(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private static bool IsRunningInGitHubActions { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

        private ITestOutputHelper OutputHelper { get; }

        public async Task WithPageAsync(
            string browserType,
            Func<IPage, Task> action,
            [CallerMemberName] string? testName = null)
        {
            using IPlaywright playwright = await Playwright.CreateAsync();

            await using IBrowser browser = await CreateBrowserAsync(playwright, browserType);

            BrowserNewPageOptions options = CreatePageOptions();

            IPage page = await browser.NewPageAsync(options);

            page.Console += (_, e) => OutputHelper.WriteLine(e.Text);
            page.PageError += (_, e) => OutputHelper.WriteLine(e);

            try
            {
                await action(page);
            }
            catch (Exception)
            {
                await TryCaptureScreenshotAsync(page, testName!, browserType);
                throw;
            }
            finally
            {
                await TryCaptureVideoAsync(page);
            }
        }

        protected virtual BrowserNewPageOptions CreatePageOptions()
        {
            var options = new BrowserNewPageOptions()
            {
                IgnoreHTTPSErrors = true,
                Locale = "en-GB",
                TimezoneId = "Europe/London",
            };

            if (IsRunningInGitHubActions)
            {
                options.RecordVideoDir = "videos";
                options.RecordVideoSize = new RecordVideoSize() { Width = 800, Height = 600 };
            }

            return options;
        }

        private static async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, string browserType)
        {
            var options = new BrowserTypeLaunchOptions();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                options.Devtools = true;
                options.Headless = false;
                options.SlowMo = 100;
            }

            return await playwright[browserType].LaunchAsync(options);
        }

        private async Task TryCaptureScreenshotAsync(
            IPage page,
            string testName,
            string browserType)
        {
            try
            {
                string os =
                    OperatingSystem.IsLinux() ? "linux" :
                    OperatingSystem.IsMacOS() ? "macos" :
                    OperatingSystem.IsWindows() ? "windows" :
                    "other";

                string utcNow = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                string path = Path.Combine("screenshots", $"{testName}_{browserType}_{os}_{utcNow}.png");

                await page.ScreenshotAsync(new PageScreenshotOptions()
                {
                    Path = path,
                });

                OutputHelper.WriteLine($"Screenshot saved to {path}.");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteLine("Failedto capture screenshot: " + ex);
            }
        }

        private async Task TryCaptureVideoAsync(IPage page)
        {
            if (IsRunningInGitHubActions)
            {
                try
                {
                    await page.CloseAsync();
                    OutputHelper.WriteLine($"Video saved to {await page.Video.PathAsync()}.");
                }
                catch (Exception ex)
                {
                    OutputHelper.WriteLine("Failed to capture video: " + ex);
                }
            }
        }
    }
}
