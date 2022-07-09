// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public abstract class ModalBase
{
    protected ModalBase(string name, ApplicationNavigator navigator)
    {
        Name = name;
        Navigator = navigator;
    }

    protected string DialogSelector => $"[data-id='modal-{Name}']";

    protected string Name { get; }

    protected ApplicationNavigator Navigator { get; }

    protected async Task CloseSelfAsync()
    {
        IElementHandle? modal = await Navigator.Page.QuerySelectorAsync(DialogSelector);
        IElementHandle? dismiss = await modal!.QuerySelectorAsync("[data-bs-dismiss='modal']");

        await dismiss!.ClickAsync();
    }
}
