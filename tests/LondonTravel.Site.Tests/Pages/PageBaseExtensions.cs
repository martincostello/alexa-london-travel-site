// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages;

public static class PageBaseExtensions
{
    public static async Task<T> NavigateAsync<T>(this T page)
        where T : PageBase
    {
        await page.NavigateToSelfAsync();
        return page;
    }
}
