// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using OpenQA.Selenium;

    public sealed class HomePage : PageBase
    {
        public HomePage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/";

        public IReadOnlyCollection<LinePreference> Lines()
        {
            return Navigator.Driver
                .FindElements(By.CssSelector("[data-line-preference]"))
                .Select((p) => new LinePreference(Navigator.Driver, p))
                .ToList();
        }

        public ManagePage Manage()
        {
            Navigator.Driver.FindElement(UserNameSelector).Click();
            return new ManagePage(Navigator);
        }

        public HomePage UpdatePreferences()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='save-preferences']")).Click();
            return new HomePage(Navigator);
        }

        public SignInPage SignIn()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='sign-in']")).Click();
            return new SignInPage(Navigator);
        }
    }
}
