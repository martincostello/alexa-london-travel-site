// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public abstract class PageBase(ApplicationNavigator navigator)
{
    protected internal ApplicationNavigator Navigator { get; } = navigator;

    protected static string UserNameSelector { get; } = "[data-id='user-name']";

    protected abstract string RelativeUri { get; }

    public async Task<bool> IsAuthenticatedAsync()
    {
        const string ContentSelector = "[data-id='content']";

        var content = await Navigator.Page.QuerySelectorAsync(ContentSelector);
        content.ShouldNotBeNull($"Could not find selector '{ContentSelector}'.");

        string? isAuthenticated = await content.GetAttributeAsync("data-authenticated");

        return bool.Parse(isAuthenticated ?? bool.FalseString);
    }

    public async Task<string> UserNameAsync()
    {
        var element = await Navigator.Page.QuerySelectorAsync(UserNameSelector);
        element.ShouldNotBeNull($"Could not find selector '{UserNameSelector}'.");

        string userName = await element.InnerTextAsync();
        userName.ShouldNotBeNull();

        return userName.Trim();
    }

    public async Task<HomePage> SignOutAsync()
    {
        await Navigator.Page.ClickAsync(Selectors.SignOut);
        return new HomePage(Navigator);
    }

    public async Task WaitForSignedInAsync()
        => await Navigator.Page.WaitForSelectorAsync(Selectors.SignOut);

    public async Task WaitForReadyAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        while (true)
        {
            try
            {
                if (await Navigator.Page.EvaluateAsync<string>("() => document.readyState") is "complete")
                {
                    return;
                }
            }
            catch (PlaywrightException)
            {
                // The execution context was destroyed by a navigation, so try again
            }

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Timed out waiting for {Navigator.Page.Url} to load.");
            }
        }
    }

    internal async Task NavigateToSelfAsync()
    {
        await Navigator.NavigateToAsync(RelativeUri);
    }

    protected async Task ClickAndWaitForNavigationAsync(string selector)
    {
        const int MaxAttempts = 3;

        var page = Navigator.Page;
        var navigated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnRequest(object? sender, IRequest request)
        {
            if (request.IsNavigationRequest)
            {
                navigated.TrySetResult();
            }
        }

        page.Request += OnRequest;

        try
        {
            for (int attempt = 1; ; attempt++)
            {
                await page.ClickAsync(selector);

                try
                {
                    await navigated.Task.WaitAsync(TimeSpan.FromSeconds(5));
                    return;
                }
                catch (TimeoutException) when (attempt < MaxAttempts)
                {
                    // Try again
                }
            }
        }
        finally
        {
            page.Request -= OnRequest;
        }
    }

    private sealed class Selectors
    {
        public const string SignOut = "[data-id='sign-out']";
    }
}
