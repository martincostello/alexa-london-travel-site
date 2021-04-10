// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using OpenQA.Selenium;

    public sealed class LinePreference
    {
        internal LinePreference(IWebDriver driver, IWebElement element)
        {
            Driver = driver;
            RootElement = element;
            InputElement = RootElement.FindElement(By.TagName("input"));
        }

        private IWebDriver Driver { get; }

        private IWebElement InputElement { get; }

        private IWebElement RootElement { get; }

        public string Id() => InputElement.GetAttribute("data-line-id");

        public bool IsSelected() => InputElement.GetAttribute("checked") != null;

        public string Name() => InputElement.GetAttribute("data-line-name");

        public LinePreference Toggle()
        {
            new OpenQA.Selenium.Interactions.Actions(Driver)
                .MoveToElement(RootElement)
                .Click(RootElement)
                .Perform();

            return this;
        }
    }
}
