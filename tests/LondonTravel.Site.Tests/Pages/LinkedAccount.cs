// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public sealed class LinkedAccount
    {
        internal LinkedAccount(ApplicationNavigator navigator, IElementHandle element)
        {
            Navigator = navigator;
            RootElement = element;
        }

        private ApplicationNavigator Navigator { get; }

        private IElementHandle RootElement { get; }

        public async Task<string> IdAsync()
            => await RootElement.GetAttributeAsync("data-provider");

        public async Task<string> NameAsync()
        {
            IElementHandle element = await RootElement.QuerySelectorAsync("span");
            string text = await element.InnerTextAsync();
            return text.Trim();
        }

        public async Task<ManagePage> RemoveAsync()
        {
            IElementHandle submit = await RootElement.QuerySelectorAsync("input[type='submit']");

            await submit.ClickAsync();

            return new ManagePage(Navigator);
        }
    }
}
