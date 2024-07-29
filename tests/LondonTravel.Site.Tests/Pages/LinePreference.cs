// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class LinePreference(IElementHandle element)
{
    private IElementHandle RootElement { get; } = element;

    public async Task<string> IdAsync() => (await GetInputAttributeAsync("data-line-id"))!;

    public async Task<bool> IsSelectedAsync() => await GetInputAttributeAsync("checked") is not null;

    public async Task<string> NameAsync() => (await GetInputAttributeAsync("data-line-name"))!;

    public async Task<LinePreference> ToggleAsync()
    {
        await RootElement.ClickAsync();
        return this;
    }

    private async Task<string?> GetInputAttributeAsync(string name)
    {
        var element = await RootElement.QuerySelectorAsync("input");

        if (element is null)
        {
            return null;
        }

        try
        {
            return await element.GetAttributeAsync(name);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
