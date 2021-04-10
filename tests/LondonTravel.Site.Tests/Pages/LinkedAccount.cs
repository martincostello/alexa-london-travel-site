// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using OpenQA.Selenium;

    public sealed class LinkedAccount
    {
        internal LinkedAccount(ApplicationNavigator navigator, IWebElement element)
        {
            Navigator = navigator;
            RootElement = element;
            NameElement = RootElement.FindElement(By.TagName("span"));
        }

        private ApplicationNavigator Navigator { get; }

        private IWebElement NameElement { get; }

        private IWebElement RootElement { get; }

        public string Id() => RootElement.GetAttribute("data-provider");

        public string Name() => NameElement.Text.Trim();

        public ManagePage Remove()
        {
            RootElement.FindElement(By.CssSelector("input[type='submit']")).Click();
            return new ManagePage(Navigator);
        }
    }
}
