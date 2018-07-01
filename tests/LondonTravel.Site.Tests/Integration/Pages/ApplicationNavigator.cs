// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Pages
{
    using System;
    using OpenQA.Selenium;

    public class ApplicationNavigator : IDisposable
    {
        private bool _disposed;

        public ApplicationNavigator(Uri baseUri, IWebDriver driver)
        {
            BaseUri = baseUri;
            Driver = driver;
        }

        ~ApplicationNavigator()
        {
            Dispose(false);
        }

        protected internal Uri BaseUri { get; }

        protected internal IWebDriver Driver { get; }

        public HomePage GoToRoot() => new HomePage(this).Navigate();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void NavigateTo(Uri relativeUri)
        {
            Uri url = new Uri(BaseUri, relativeUri);
            Driver.Navigate().GoToUrl(url);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Driver?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
