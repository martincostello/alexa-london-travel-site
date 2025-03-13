// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class DeleteModal(ApplicationNavigator navigator) : ModalBase("delete-account", navigator)
{
    public async Task<ManagePage> CloseAsync()
    {
        await CloseSelfAsync();
        return new ManagePage(Navigator);
    }

    public async Task<HomePage> ConfirmAsync()
    {
        await Navigator.Page.ClickAsync("[data-id='delete-account-confirm']");
        var page = new HomePage(Navigator);

        await page.WaitForSignedOutAsync();

        return page;
    }
}
