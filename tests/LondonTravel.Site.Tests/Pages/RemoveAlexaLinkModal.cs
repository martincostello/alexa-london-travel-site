// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System;
    using System.Threading;
    using OpenQA.Selenium;

    public sealed class RemoveAlexaLinkModal : ModalBase
    {
        public RemoveAlexaLinkModal(ApplicationNavigator navigator)
            : base("remove-alexa", navigator)
        {
        }

        public ManagePage Close()
        {
            CloseSelf();
            return new ManagePage(Navigator);
        }

        public ManagePage Confirm()
        {
            IWebElement button = Navigator.Driver.FindElement(By.CssSelector("[data-id='remove-alexa-confirm']"));

            // Wait for the JavaScript to enable the button
            using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                while (button.GetAttribute("disabled") != null)
                {
                    source.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            button.Click();
            return new ManagePage(Navigator);
        }
    }
}
