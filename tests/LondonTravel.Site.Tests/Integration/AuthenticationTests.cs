// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A class containing tests for authentication providers in the website.
    /// </summary>
    public class AuthenticationTests : BrowserTest
    {
        public AuthenticationTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        [NotCIFact]
        public async Task Can_Load_Homepage()
        {
            using (var driver = CreateWebDriver())
            {
                driver.Navigate().GoToUrl(Fixture.ServerAddress);

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
