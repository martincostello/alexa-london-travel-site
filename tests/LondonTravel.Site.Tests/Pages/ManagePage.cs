// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site.Pages;

public sealed class ManagePage(ApplicationNavigator navigator) : PageBase(navigator)
{
    protected override string RelativeUri => "/manage/";

    public async Task<DeleteModal> DeleteAccountAsync()
    {
        await Navigator.Page.ClickAsync("[data-id='delete-account']");

        var modal = new DeleteModal(Navigator);
        await modal.WaitForLoadedAsync();

        return modal;
    }

    public async Task<bool> IsLinkedToAlexaAsync()
    {
        var linked = await Navigator.Page.QuerySelectorAsync(Selectors.Link);

        string? value = await linked!.GetAttributeAsync("data-is-linked");

        return bool.Parse(value ?? bool.FalseString);
    }

    public async Task<IReadOnlyList<LinkedAccount>> LinkedAccountsAsync()
    {
        var elements = await Navigator.Page.QuerySelectorAllAsync(Selectors.LinkedAccount);

        return [.. elements.Select((p) => new LinkedAccount(Navigator, p))];
    }

    public async Task<ManagePage> SignInWithGoogleAsync()
        => await SignInWithProviderAsync("google");

    public async Task<ManagePage> SignInWithProviderAsync(string name)
    {
        await Navigator.Page.ClickAsync($"[data-id='sign-in-{name}']");
        return new ManagePage(Navigator);
    }

    public async Task<RemoveAlexaLinkModal> UnlinkAlexaAsync()
    {
        await Navigator.Page.ClickAsync(Selectors.Unlink);

        var modal = new RemoveAlexaLinkModal(Navigator);
        await modal.WaitForLoadedAsync();

        return modal;
    }

    public async Task WaitForLinkedAccountCountAsync(int count)
    {
        await Assertions.Expect(Navigator.Page.Locator(Selectors.LinkedAccount))
                        .ToHaveCountAsync(count);
    }

    public async Task WaitForLinkedAsync()
        => await Navigator.Page.WaitForSelectorAsync(Selectors.Unlink);

    public async Task WaitForUnlinkedAsync()
        => await Navigator.Page.WaitForSelectorAsync(Selectors.Link);

    private sealed class Selectors
    {
        public const string Link = "[data-id='alexa-link']";

        public const string LinkedAccount = "[data-linked-account]";

        public const string Unlink = "[data-id='remove-alexa-link']";
    }
}
