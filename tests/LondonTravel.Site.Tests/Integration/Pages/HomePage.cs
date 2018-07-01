// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Pages
{
    using OpenQA.Selenium;

    public sealed class HomePage : PageBase
    {
        public HomePage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/";

        public SignInPage SignIn()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='sign-in']")).Click();
            return new SignInPage(Navigator);
        }

        public HomePage SignOut()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='sign-out']")).Click();
            return new HomePage(Navigator);
        }
    }
}
