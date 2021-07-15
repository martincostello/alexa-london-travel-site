// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    public sealed class SignInPage : PageBase
    {
        public SignInPage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/account/sign-in/";

        public async Task<HomePage> SignInWithAmazonAsync() => await SignInWithProviderAsync("amazon");

        public async Task<HomePage> SignInWithProviderAsync(string name)
        {
            await Navigator.Page.ClickAsync($"[data-id='sign-in-{name}']");
            return new HomePage(Navigator);
        }
    }
}
