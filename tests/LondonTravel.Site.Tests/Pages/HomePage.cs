// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class HomePage(ApplicationNavigator navigator) : PageBase(navigator)
{
    protected override string RelativeUri => "/";

    public async Task<IReadOnlyList<LinePreference>> LinesAsync()
    {
        var elements = await Navigator.Page.QuerySelectorAllAsync(Selectors.Lines);

        return [.. elements.Select((p) => new LinePreference(p))];
    }

    public async Task<ManagePage> ManageAsync()
    {
        await Navigator.Page.ClickAsync(UserNameSelector);
        return new ManagePage(Navigator);
    }

    public async Task<HomePage> UpdatePreferencesAsync()
    {
        await Navigator.Page.ClickAsync("[data-id='save-preferences']");
        var page = new HomePage(Navigator);

        await page.WaitForSignedInAsync();

        return page;
    }

    public async Task<SignInPage> SignInAsync()
    {
        await Navigator.Page.ClickAsync(Selectors.SignIn);
        return new SignInPage(Navigator);
    }

    public async Task WaitForLinesAsync()
        => await Navigator.Page.WaitForSelectorAsync(Selectors.Lines);

    public async Task WaitForSignedOutAsync()
        => await Navigator.Page.WaitForSelectorAsync(Selectors.SignIn);

    private sealed class Selectors
    {
        public const string Lines = "[data-line-preference]";
        public const string SignIn = "[data-id='sign-in']";
    }
}
