// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Pages
{
    using OpenQA.Selenium;

    public abstract class ModalBase
    {
        protected ModalBase(string name, ApplicationNavigator navigator)
        {
            Name = name;
            Navigator = navigator;
        }

        protected By DialogSelector => By.CssSelector($"[data-id='modal-{Name}']");

        protected string Name { get; }

        protected ApplicationNavigator Navigator { get; }

        protected void CloseSelf()
        {
            Navigator.Driver.FindElement(DialogSelector).FindElement(By.CssSelector("[data-dismiss='modal']")).Click();
        }
    }
}
