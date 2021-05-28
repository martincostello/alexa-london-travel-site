// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System.Threading.Tasks;

    public abstract class PageBase
    {
        protected PageBase(ApplicationNavigator navigator)
        {
            Navigator = navigator;
        }

        protected internal ApplicationNavigator Navigator { get; }

        protected static string UserNameSelector { get; } = "[data-id='user-name']";

        protected abstract string RelativeUri { get; }

        public async Task<bool> IsAuthenticatedAsync()
            => bool.Parse(await (await Navigator.Page.QuerySelectorAsync("[data-id='content']")).GetAttributeAsync("data-authenticated"));

        public async Task<string> UserNameAsync()
            => (await (await Navigator.Page.QuerySelectorAsync(UserNameSelector)).InnerTextAsync()).Trim();

        public async Task<HomePage> SignOutAsync()
        {
            await Navigator.Page.ClickAsync("[data-id='sign-out']");
            return new HomePage(Navigator);
        }

        internal async Task NavigateToSelfAsync()
        {
            await Navigator.NavigateToAsync(RelativeUri);
        }
    }
}
