// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Pages;
using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.EndToEnd;

public class SocialLoginTests(WebsiteFixture fixture, ITestOutputHelper outputHelper) : BrowserEndToEndTest(fixture, outputHelper)
{
    [SkippableFact]
    public async Task Can_Sign_In_With_Google()
    {
        Skip.IfNot(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")),
            "Sign-in blocked when run in GitHub Actions.");

        await SignInWithSocialProviderAsync(
            "Google",
            async (driver, userName, password) =>
            {
                await SignInAsync(
                    driver,
                    "input[type=email]",
                    "input[type=password]",
                    (userName, password),
                    sendEnterAfterUserName: true);
            });
    }

    [SkippableFact]
    public async Task Can_Sign_In_With_Microsoft_Account()
    {
        Skip.IfNot(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")),
            "Sign-in is being flaky in GitHub Actions.");

        await SignInWithSocialProviderAsync(
            "Microsoft",
            async (driver, userName, password) =>
            {
                await SignInAsync(
                    driver,
                    "input[type=email]",
                    "input[type=password]",
                    (userName, password),
                    sendEnterAfterUserName: true);
            });
    }

    [SkippableFact]
    public async Task Can_Sign_In_With_Twitter()
    {
        Skip.IfNot(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")),
            "Sign-in blocked when run in GitHub Actions.");

        await SignInWithSocialProviderAsync(
            "Twitter",
            async (driver, userName, password) =>
            {
                await SignInAsync(
                    driver,
                    "id=username_or_email",
                    "input[type=password]",
                    (userName, password));
            });
    }

    private async Task SignInWithSocialProviderAsync(string providerName, Func<IPage, string, string, Task> signIn)
    {
        string? userName = Environment.GetEnvironmentVariable($"WEBSITE_USER_{providerName.ToUpperInvariant()}_USERNAME");
        string? password = Environment.GetEnvironmentVariable($"WEBSITE_USER_{providerName.ToUpperInvariant()}_PASSWORD");

        Skip.If(string.IsNullOrWhiteSpace(userName), $"No {providerName} username is configured.");
        Skip.If(string.IsNullOrWhiteSpace(password), $"No {providerName} password is configured.");

        // Arrange
        await AtPageAsync<HomePage>(
            BrowserType.Chromium,
            null,
            async (page) =>
            {
                page = await page.SignInAsync()
                                 .ThenAsync((p) => p.SignInWithProviderAsync(providerName.ToLowerInvariant()));

                await signIn(page.Navigator.Page, userName, password);

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.UserNameAsync().ShouldNotBeNullOrWhiteSpace();

                // Act
                page = await page.SignOutAsync();

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeFalse();
            });
    }

    private async Task SignInAsync(
        IPage page,
        string userNameSelector,
        string passwordSelector,
        (string UserName, string Password) credentials,
        bool sendEnterAfterUserName = false)
    {
        await page.WaitForURLAsync((p) => !p.StartsWith(Fixture.ServerAddress.ToString(), StringComparison.Ordinal));

        var userName = await page.WaitForSelectorAsync(userNameSelector);

        await userName!.FillAsync(credentials.UserName);

        if (sendEnterAfterUserName)
        {
            await page.Keyboard.PressAsync("Enter");
        }

        // HACK Microsoft authentication has a transition that
        // makes Playwright type the password in too soon.
        await Task.Delay(TimeSpan.FromSeconds(2));

        var password = await page.WaitForSelectorAsync(passwordSelector);

        await password!.FillAsync(credentials.Password);
        await page.Keyboard.PressAsync("Enter");

        await page.WaitForURLAsync((p) => p.StartsWith(ServerAddress.ToString(), StringComparison.OrdinalIgnoreCase));
    }
}
