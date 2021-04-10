// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Pages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A class containing tests for the integration with Alexa.
    /// </summary>
    public class AlexaTests : BrowserIntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        public AlexaTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
            Fixture.Services!.GetRequiredService<InMemoryDocumentStore>().Clear();
        }

        [Fact]
        public void Can_Authorize_Alexa()
        {
            // Arrange
            WithNavigator(
                (navigator) =>
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
                });
        }

        [Fact]
        public async Task Can_Get_Preferences_From_Api()
        {
            // Arrange
            await WithNavigatorAsync(
                async (navigator) =>
                {
                    var page = new SignInPage(navigator)
                        .Navigate()
                        .SignInWithAmazon()
                        .Manage();

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.IsLinkedToAlexa().ShouldBeFalse();

                    // Arrange
                    Uri relativeUri = BuildAuthorizationUri(navigator);

                    // Act
                    navigator.NavigateTo(relativeUri);

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.IsLinkedToAlexa().ShouldBeTrue();

                    // Arrange
                    AuthenticationHeaderValue authorization = ParseAuthorization(navigator.Driver.Url);

                    // Act
                    using var firstResult = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                    // Assert
                    firstResult.RootElement.GetString("userId").ShouldNotBeNullOrWhiteSpace();
                    firstResult.RootElement.GetStringArray("favoriteLines").ShouldBe(Array.Empty<string>());

                    // Arrange
                    HomePage homepage = navigator.GoToRoot();

                    homepage
                        .Lines()
                        .First((p) => p.Name() == "District")
                        .Toggle();

                    homepage.UpdatePreferences();

                    // Act
                    using var secondResult = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                    // Assert
                    secondResult.RootElement.GetString("userId").ShouldNotBeNullOrWhiteSpace();
                    secondResult.RootElement.GetStringArray("favoriteLines").ShouldBe(new[] { "district" });

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
                });
        }

        private static Uri BuildAuthorizationUri(ApplicationNavigator navigator)
        {
            var queryString = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["client_id"] = "alexa-london-travel",
                ["redirect_uri"] = new Uri(navigator.BaseUri, new Uri("/manage/", UriKind.Relative)).ToString(),
                ["response_type"] = "token",
                ["state"] = "my_state",
            };

            string uriString = QueryHelpers.AddQueryString("/alexa/authorize/", queryString);

            return new Uri(uriString, UriKind.Relative);
        }

        private static AuthenticationHeaderValue ParseAuthorization(string url)
        {
            var hash = new UriBuilder(url).Fragment;

            Dictionary<string, StringValues> values = QueryHelpers.ParseQuery(hash);

            string scheme = values["token_type"];
            string parameter = values["access_token"];

            return new AuthenticationHeaderValue(scheme, parameter);
        }

        private async Task<JsonDocument> GetPreferencesAsync(AuthenticationHeaderValue authorization, HttpStatusCode expected)
        {
            // Arrange
            using var client = Fixture.CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = authorization;

            // Act
            using var response = await client.GetAsync("/api/preferences");

            // Assert
            response.StatusCode.ShouldBe(expected);

            return await response.ReadAsJsonDocumentAsync();
        }
    }
}
