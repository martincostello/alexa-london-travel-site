// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public sealed class LinePreference
    {
        internal LinePreference(IElementHandle element)
        {
            RootElement = element;
        }

        private IElementHandle RootElement { get; }

        public async Task<string> IdAsync() => (await GetInputAttributeAsync("data-line-id")) !;

        public async Task<bool> IsSelectedAsync() => await GetInputAttributeAsync("checked") != null;

        public async Task<string> NameAsync() => (await GetInputAttributeAsync("data-line-name")) !;

        public async Task<LinePreference> ToggleAsync()
        {
            await RootElement.ClickAsync();
            return this;
        }

        private async Task<string?> GetInputAttributeAsync(string name)
        {
            IElementHandle element = await RootElement.QuerySelectorAsync("input");

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
}
