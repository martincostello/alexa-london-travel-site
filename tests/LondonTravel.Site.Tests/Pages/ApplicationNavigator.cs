// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public class ApplicationNavigator(Uri baseUri, IPage page)
{
    protected internal Uri BaseUri { get; } = baseUri;

    protected internal IPage Page { get; } = page;

    public async Task<HomePage> GoToRootAsync() => await new HomePage(this).NavigateAsync();

    public async Task NavigateToAsync(string relativeUri)
    {
        var url = new Uri(BaseUri, relativeUri);

        await Page.GotoAsync(url.ToString());
    }
}
