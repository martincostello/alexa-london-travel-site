// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace MartinCostello.LondonTravel.Site.EndToEnd;

[Collection<WebsiteCollection>]
[Trait("Category", "EndToEnd")]
public class ApiTests(WebsiteFixture fixture)
{
    [Fact]
    public async Task Cannot_Get_Preferences_Unauthenticated()
    {
        // Arrange
        using var client = fixture.CreateClient();

        // Act
        using var response = await client.GetAsync("/api/preferences", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("No access token specified.");
        result.RootElement.GetInt32("statusCode").ShouldBe(StatusCodes.Status401Unauthorized);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBeEmpty();
    }

    [Fact]
    public async Task Cannot_Get_Preferences_With_Invalid_Token()
    {
        // Arrange
        string accessToken = Guid.NewGuid().ToString();

        using var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

        // Act
        using var response = await client.GetAsync("/api/preferences", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("Unauthorized.");
        result.RootElement.GetInt32("statusCode").ShouldBe(StatusCodes.Status401Unauthorized);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBeEmpty();
    }
}
