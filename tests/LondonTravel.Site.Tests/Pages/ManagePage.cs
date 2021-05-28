// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public sealed class ManagePage : PageBase
    {
        public ManagePage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/manage/";

        public async Task<DeleteModal> DeleteAccountAsync()
        {
            await Navigator.Page.ClickAsync("[data-id='delete-account']");
            return new DeleteModal(Navigator);
        }

        public async Task<bool> IsLinkedToAlexaAsync()
        {
            IElementHandle linked = await Navigator.Page.QuerySelectorAsync("[data-id='alexa-link']");

            string value = await linked.GetAttributeAsync("data-is-linked");

            return bool.Parse(value);
        }

        public async Task<IReadOnlyList<LinkedAccount>> LinkedAccountsAsync()
        {
            var elements = await Navigator.Page.QuerySelectorAllAsync("[data-linked-account]");

            return elements
                .Select((p) => new LinkedAccount(Navigator, p))
                .ToList();
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
            await Navigator.Page.ClickAsync("[data-id='remove-alexa-link']");
            return new RemoveAlexaLinkModal(Navigator);
        }
    }
}
