// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public class ApplicationNavigator
    {
        public ApplicationNavigator(Uri baseUri, IPage page)
        {
            BaseUri = baseUri;
            Page = page;
        }

        protected internal Uri BaseUri { get; }

        protected internal IPage Page { get; }

        public async Task<HomePage> GoToRootAsync() => await new HomePage(this).NavigateAsync();

        public async Task NavigateToAsync(string relativeUri)
        {
            var url = new Uri(BaseUri, relativeUri);

            await Page.GotoAsync(url.ToString());
        }
    }
}
