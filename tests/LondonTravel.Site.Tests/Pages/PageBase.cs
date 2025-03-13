// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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

    internal async Task NavigateToSelfAsync()
    {
        await Navigator.NavigateToAsync(RelativeUri);
    }

    private sealed class Selectors
    {
        public const string SignOut = "[data-id='sign-out']";
    }
}
