// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class HomePage(ApplicationNavigator navigator) : PageBase(navigator)
{
    protected override string RelativeUri => "/";

    public async Task<IReadOnlyList<LinePreference>> LinesAsync()
    {
        var elements = await Navigator.Page.QuerySelectorAllAsync("[data-line-preference]");

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
        return new HomePage(Navigator);
    }

    public async Task<SignInPage> SignInAsync()
    {
        await Navigator.Page.ClickAsync("[data-id='sign-in']");
        return new SignInPage(Navigator);
    }
}
