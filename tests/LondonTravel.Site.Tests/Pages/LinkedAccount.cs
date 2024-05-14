// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class LinkedAccount(ApplicationNavigator navigator, IElementHandle element)
{
    private ApplicationNavigator Navigator { get; } = navigator;

    private IElementHandle RootElement { get; } = element;

    public async Task<string?> IdAsync()
        => await RootElement.GetAttributeAsync("data-provider");

    public async Task<string> NameAsync()
    {
        var element = await RootElement.QuerySelectorAsync("span[class='brand-name']");
        string text = await element!.InnerTextAsync();
        return text.Trim();
    }

    public async Task<ManagePage> RemoveAsync()
    {
        var submit = await RootElement.QuerySelectorAsync("input[type='submit']");

        await submit!.ClickAsync();

        return new ManagePage(Navigator);
    }
}
