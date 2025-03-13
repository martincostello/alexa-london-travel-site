// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class RemoveAlexaLinkModal(ApplicationNavigator navigator) : ModalBase("remove-alexa", navigator)
{
    public async Task<ManagePage> CloseAsync()
    {
        await CloseSelfAsync();
        return new ManagePage(Navigator);
    }

    public async Task<ManagePage> ConfirmAsync()
    {
        await Navigator.Page.ClickAsync("[data-id='remove-alexa-confirm']");
        var page = new ManagePage(Navigator);

        await page.WaitForUnlinkedAsync();

        return page;
    }
}
