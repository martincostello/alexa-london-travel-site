// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using OpenQA.Selenium;

    public sealed class ManagePage : PageBase
    {
        public ManagePage(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string RelativeUri => "/manage/";

        public DeleteModal DeleteAccount()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='delete-account']")).Click();
            return new DeleteModal(Navigator);
        }

        public bool IsLinkedToAlexa() =>
            bool.Parse(Navigator.Driver.FindElement(By.CssSelector("[data-id='alexa-link']")).GetAttribute("data-is-linked"));

        public IReadOnlyCollection<LinkedAccount> LinkedAccounts()
        {
            return Navigator.Driver
                .FindElements(By.CssSelector("[data-linked-account]"))
                .Select((p) => new LinkedAccount(Navigator, p))
                .ToList();
        }

        public ManagePage SignInWithGoogle() => SignInWithProvider("google");

        public ManagePage SignInWithProvider(string name)
        {
            Navigator.Driver.FindElement(By.CssSelector($"[data-id='sign-in-{name}']")).Click();
            return new ManagePage(Navigator);
        }

        public RemoveAlexaLinkModal UnlinkAlexa()
        {
            Navigator.Driver.FindElement(By.CssSelector("[data-id='remove-alexa-link']")).Click();
            return new RemoveAlexaLinkModal(Navigator);
        }
    }
}
