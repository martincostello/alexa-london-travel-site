// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using MartinCostello.LondonTravel.Site.Pages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Site.Integration;

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

    [Theory]
    [ClassData(typeof(BrowsersTestData))]
    public async Task Can_Authorize_Alexa(string browserType, string? browserChannel)
    {
        // Arrange
        await WithNavigatorAsync(
            browserType,
            browserChannel,
            async (navigator) =>
            {
                string relativeUri = BuildAuthorizationUri(navigator);

                // Act
                await navigator.NavigateToAsync(relativeUri);

                var page = await new SignInPage(navigator)
                    .SignInWithAmazonAsync()
                    .ThenAsync((p) => p.ManageAsync());

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.IsLinkedToAlexaAsync().ShouldBeTrue();
            });
    }

    [Theory]
    [ClassData(typeof(BrowsersTestData))]
    public async Task Can_Get_Preferences_From_Api(string browserType, string? browserChannel)
    {
        // Arrange
        await WithNavigatorAsync(
            browserType,
            browserChannel,
            async (navigator) =>
            {
                var page = await new SignInPage(navigator)
                    .NavigateAsync()
                    .ThenAsync((p) => p.SignInWithAmazonAsync())
                    .ThenAsync((p) => p.ManageAsync());

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.IsLinkedToAlexaAsync().ShouldBeFalse();

                // Arrange
                string relativeUri = BuildAuthorizationUri(navigator);

                // Act
                await navigator.NavigateToAsync(relativeUri);

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.IsLinkedToAlexaAsync().ShouldBeTrue();

                // Arrange
                var authorization = ParseAuthorization(navigator.Page.Url);

                // Act
                using var firstResult = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                // Assert
                firstResult.RootElement.GetString("userId").ShouldNotBeNullOrWhiteSpace();
                firstResult.RootElement.GetStringArray("favoriteLines").ShouldBe([]);

                // Arrange
                var homepage = await navigator.GoToRootAsync();

                var lines = await homepage.LinesAsync();

                for (int i = 0; i < lines.Count; i++)
                {
                    if (string.Equals(await lines[i].NameAsync(), "District", StringComparison.Ordinal))
                    {
                        await lines[i].ToggleAsync();
                        break;
                    }
                }

                await homepage.UpdatePreferencesAsync();

                // Act
                using var secondResult = await GetPreferencesAsync(authorization, HttpStatusCode.OK);

                // Assert
                secondResult.RootElement.GetString("userId").ShouldNotBeNullOrWhiteSpace();
                secondResult.RootElement.GetStringArray("favoriteLines").ShouldBe(["district"]);

                // Arrange
                page = await homepage.ManageAsync();

                // Act
                page = await page
                    .UnlinkAlexaAsync()
                    .ThenAsync((p) => p.CloseAsync());

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.IsLinkedToAlexaAsync().ShouldBeTrue();

                // Act
                page = await page
                    .UnlinkAlexaAsync()
                    .ThenAsync((p) => p.ConfirmAsync());

                // Assert
                await page.IsAuthenticatedAsync().ShouldBeTrue();
                await page.IsLinkedToAlexaAsync().ShouldBeFalse();

                // Act and Assert
                await GetPreferencesAsync(authorization, HttpStatusCode.Unauthorized);
            });
    }

    private static string BuildAuthorizationUri(ApplicationNavigator navigator)
    {
        var queryString = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["client_id"] = "alexa-london-travel",
            ["redirect_uri"] = new Uri(navigator.BaseUri, new Uri("/manage/", UriKind.Relative)).ToString(),
            ["response_type"] = "token",
            ["state"] = "my_state",
        };

        return QueryHelpers.AddQueryString("/alexa/authorize/", queryString);
    }

    private static AuthenticationHeaderValue ParseAuthorization(string url)
    {
        string? hash = new UriBuilder(url).Fragment;

        var values = QueryHelpers.ParseQuery(hash);

        string? scheme = values["token_type"];
        string? parameter = values["access_token"];

        return new AuthenticationHeaderValue(scheme!, parameter);
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
