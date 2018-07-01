// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Pages
{
    using OpenQA.Selenium;

    public sealed class SignInPage : PageBase
    {
        public SignInPage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/account/sign-in/";

        public HomePage SignInWithAmazon() => SignInWithProvider("amazon");

        public HomePage SignInWithProvider(string name)
        {
            Navigator.Driver.FindElement(By.CssSelector($"[data-id='sign-in-{name}']")).Click();
            return new HomePage(Navigator);
        }
    }
}
