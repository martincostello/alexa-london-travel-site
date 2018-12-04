// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Integration.Builders;
    using MartinCostello.LondonTravel.Site.Integration.Pages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using OpenQA.Selenium;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A class containing tests for the integration with Alexa.
    /// </summary>
    public class AlexaTests : BrowserTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        public AlexaTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task Can_Authorize_Alexa_And_Get_Preferences_From_Api()
        {
            // Arrange
            new AuthenticationInterceptionBuilder(Fixture.Interceptor).ForAmazon();

            await WithNavigatorAsync(
                async (navigator) =>
                {
                    Uri relativeUri = BuildAuthorizationUri(navigator);

                    // Act
                    navigator.NavigateTo(relativeUri);

                    var page = new SignInPage(navigator)
                        .SignInWithAmazon()
                        .Manage();

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.IsLinkedToAlexa().ShouldBeTrue();

                    // Arrange
                    AuthenticationHeaderValue authorization = ParseAuthorization(navigator.Driver);

                    // Act
                    JObject result = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                    // Assert
                    result.Value<string>("userId").ShouldNotBeNullOrWhiteSpace();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldNotBeNull();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldBeEmpty();

                    // Arrange
                    HomePage homepage = navigator.GoToRoot();

                    homepage
                        .Lines()
                        .First((p) => p.Name() == "District")
                        .Toggle();

                    homepage.UpdatePreferences();

                    // Act
                    result = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                    // Assert
                    result.Value<string>("userId").ShouldNotBeNullOrWhiteSpace();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldNotBeNull();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldBe(new[] { "district" });

                    // Arrange
                    page = homepage.Manage();

                    // Act
                    page.UnlinkAlexa()
                        .Close();

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.IsLinkedToAlexa().ShouldBeTrue();

                    // Act
                    page.UnlinkAlexa()
                        .Confirm();

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.IsLinkedToAlexa().ShouldBeFalse();

                    // Act and Assert
                    await GetPreferencesAsync(authorization, HttpStatusCode.Unauthorized);
                },
                collectPerformanceLogs: true);
        }

        private static Uri BuildAuthorizationUri(ApplicationNavigator navigator)
        {
            var queryString = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "client_id", "alexa-london-travel" },
                { "redirect_uri", new Uri(navigator.BaseUri, new Uri("/manage/", UriKind.Relative)).ToString() },
                { "response_type", "token" },
                { "state", "my_state" },
            };

            string uriString = QueryHelpers.AddQueryString("/alexa/authorize/", queryString);

            return new Uri(uriString, UriKind.Relative);
        }

        private static AuthenticationHeaderValue ParseAuthorization(IWebDriver driver)
        {
            string driverUrl = driver.Url;

            // Trawl the performance logs to find the redirection that loaded the current
            // page as that will have contained the parameters with the token in its hash.
            var logs = driver.Manage().Logs.GetLog("performance")
                .Select((p) => JObject.Parse(p.Message))
                .Select((p) => p.Value<JObject>("message"))
                .Where((p) => p.Value<string>("method") == "Network.requestWillBeSent")
                .Where((p) => p["params"] != null)
                .Select((p) => p["params"])
                .Where((p) => p["redirectResponse"] != null)
                .Where((p) => p["request"] != null)
                .Where((p) => p["request"].Value<string>("url") == driverUrl)
                .Select((p) => p["redirectResponse"]["headers"])
                .ToList();

            // Handle casing differences in HTTP response header casing in the logs
            string url = logs
                .Select((p) => p.Value<string>("Location"))
                .LastOrDefault();

            if (url == null)
            {
                url = logs
                    .Select((p) => p.Value<string>("location"))
                    .LastOrDefault();
            }

            if (url == null)
            {
                throw new InvalidOperationException("Failed to parse browser performance log for authorization URL.");
            }

            return ParseAuthorization(url);
        }

        private static AuthenticationHeaderValue ParseAuthorization(string url)
        {
            int index = url.IndexOf('#', StringComparison.Ordinal);
            string hash = url.Substring(index + 1);

            Dictionary<string, StringValues> values = QueryHelpers.ParseQuery(hash);

            string scheme = values["token_type"];
            string parameter = values["access_token"];

            return new AuthenticationHeaderValue(scheme, parameter);
        }

        private async Task<JObject> GetPreferencesAsync(AuthenticationHeaderValue authorization, HttpStatusCode expected)
        {
            // Arrange
            using (var client = Fixture.CreateHttpClient())
            {
                client.DefaultRequestHeaders.Authorization = authorization;

                // Act
                using (var response = await client.GetAsync("/api/preferences"))
                {
                    // Assert
                    response.StatusCode.ShouldBe(expected);

                    return await response.ReadAsObjectAsync();
                }
            }
        }
    }
}
