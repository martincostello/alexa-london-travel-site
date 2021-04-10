// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System;
    using OpenQA.Selenium;

    public abstract class PageBase
    {
        protected PageBase(ApplicationNavigator navigator)
        {
            Navigator = navigator;
        }

        protected internal ApplicationNavigator Navigator { get; }

        protected abstract string RelativeUri { get; }

        protected By UserNameSelector { get; } = By.CssSelector("[data-id='user-name']");

        public bool IsAuthenticated() => bool.Parse(Navigator.Driver.FindElement(By.CssSelector("[data-id='content']")).GetAttribute("data-authenticated"));

        public string UserName() => Navigator.Driver.FindElement(UserNameSelector).Text;

        public HomePage SignOut()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='sign-out']")).Click();
            return new HomePage(Navigator);
        }

        internal void NavigateToSelf()
        {
            Uri relativeUri = new Uri(RelativeUri, UriKind.Relative);
            Navigator.NavigateTo(relativeUri);
        }
    }
}
