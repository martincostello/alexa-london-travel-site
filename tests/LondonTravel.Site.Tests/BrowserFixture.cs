// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site;

public class BrowserFixture
{
    public BrowserFixture(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    public bool CaptureTrace { get; set; }

    public bool CaptureVideo { get; set; }

    private ITestOutputHelper OutputHelper { get; }

    public async Task WithPageAsync(
        string browserType,
        Func<IPage, Task> action,
        [CallerMemberName] string? testName = null)
    {
        using IPlaywright playwright = await Playwright.CreateAsync();

        await using IBrowser browser = await CreateBrowserAsync(playwright, browserType);

        BrowserNewContextOptions options = CreatePageOptions();
        await using IBrowserContext context = await browser.NewContextAsync(options);

        if (CaptureTrace)
        {
            await context.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
                Title = testName!,
            });
        }

        IPage page = await context.NewPageAsync();

        page.Console += (_, e) => OutputHelper.WriteLine(e.Text);
        page.PageError += (_, e) => OutputHelper.WriteLine(e);

        try
        {
            await action(page);
        }
        catch (Exception)
        {
            if (CaptureTrace)
            {
                string traceName = GenerateFileName(testName!, browserType, ".zip");
                string path = Path.Combine("traces", traceName);

                await context.Tracing.StopAsync(new() { Path = path });
            }

            await TryCaptureScreenshotAsync(page, testName!, browserType);
            throw;
        }
        finally
        {
            await TryCaptureVideoAsync(page, testName!, browserType);
        }
    }

    protected virtual BrowserNewContextOptions CreatePageOptions()
    {
        var options = new BrowserNewContextOptions()
        {
            IgnoreHTTPSErrors = true,
            Locale = "en-GB",
            TimezoneId = "Europe/London",
        };

        if (CaptureVideo)
        {
            options.RecordVideoDir = "videos";
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

        string[] split = browserType.Split(':');

        browserType = split[0];

        if (split.Length > 1)
        {
            options.Channel = split[1];
        }

        return await playwright[browserType].LaunchAsync(options);
    }

    private static string GenerateFileName(string testName, string browserType, string extension)
    {
        string os =
            OperatingSystem.IsLinux() ? "linux" :
            OperatingSystem.IsMacOS() ? "macos" :
            OperatingSystem.IsWindows() ? "windows" :
            "other";

        browserType = browserType.Replace(':', '_');

        string utcNow = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
        return $"{testName}_{browserType}_{os}_{utcNow}{extension}";
    }

    private async Task TryCaptureScreenshotAsync(
        IPage page,
        string testName,
        string browserType)
    {
        try
        {
            string fileName = GenerateFileName(testName, browserType, ".png");
            string path = Path.GetFullPath(Path.Combine("screenshots", fileName));

            await page.ScreenshotAsync(new PageScreenshotOptions()
            {
                Path = path,
            });

            OutputHelper.WriteLine($"Screenshot saved to {path}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture screenshot: " + ex);
        }
    }

    private async Task TryCaptureVideoAsync(
        IPage page,
        string testName,
        string browserType)
    {
        if (!CaptureVideo || page.Video is null)
        {
            return;
        }

        try
        {
            await page.CloseAsync();

            string videoSource = await page.Video.PathAsync();

            string? directory = Path.GetDirectoryName(videoSource);
            string? extension = Path.GetExtension(videoSource);

            string fileName = GenerateFileName(testName, browserType, extension!);

            string videoDestination = Path.Combine(directory!, fileName);

            File.Move(videoSource, videoDestination);

            OutputHelper.WriteLine($"Video saved to {videoDestination}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture video: " + ex);
        }
    }
}
