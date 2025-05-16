// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Validations;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class containing tests for the API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApiTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
[Collection<TestServerCollection>]
public class ApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    private const string Scheme = "bearer";
    private const string RequestUri = "/api/preferences";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Cannot_Get_Preferences_Unauthenticated(string? value)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        if (value is not null)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Auhorization", value);
        }

        // Act
        using var response = await client.GetAsync(RequestUri, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("No access token specified.");
        result.RootElement.GetInt32("statusCode").ShouldBe(401);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("not;auth")]
    public async Task Cannot_Get_Preferences_With_Invalid_Token(string value)
    {
        // Arrange
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", value);

        // Act
        using var response = await client.GetAsync(RequestUri, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("Unauthorized.");
        result.RootElement.GetInt32("statusCode").ShouldBe(401);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBe(["The provided authorization value is not valid."]);
    }

    [Theory]
    [InlineData("something")]
    [InlineData("unknown")]
    public async Task Cannot_Get_Preferences_With_Invalid_Scheme_Value(string value)
    {
        // Arrange
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(value, "token");

        // Act
        using var response = await client.GetAsync(RequestUri, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("Unauthorized.");
        result.RootElement.GetInt32("statusCode").ShouldBe(401);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBe(["Only the bearer authorization scheme is supported."]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("foo")]
    [InlineData("bar")]
    public async Task Cannot_Get_Preferences_With_Invalid_Parameter_Value(string value)
    {
        // Arrange
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, value);

        // Act
        using var response = await client.GetAsync(RequestUri, CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using var result = await response.ReadAsJsonDocumentAsync();

        result.RootElement.GetString("requestId").ShouldNotBeNullOrWhiteSpace();
        result.RootElement.GetString("message").ShouldBe("Unauthorized.");
        result.RootElement.GetInt32("statusCode").ShouldBe(401);
        result.RootElement.GetStringArray("details").ShouldNotBeNull();
        result.RootElement.GetStringArray("details").ShouldBeEmpty();
    }

    [Fact]
    public async Task Schema_Has_No_Validation_Warnings()
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();
        using var client = Fixture.CreateClient();

        // Act
        using var schema = await client.GetStreamAsync("/openapi/api.json", CancellationToken);

        // Assert
        var actual = await OpenApiDocument.LoadAsync(schema, "json", cancellationToken: CancellationToken);

        actual.ShouldNotBeNull();
        actual.Document.ShouldNotBeNull();
        actual.Diagnostic.ShouldNotBeNull();
        actual.Diagnostic.Errors.ShouldBeEmpty();

        var errors = actual.Document.Validate(ruleSet);
        errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task Schema_Is_Correct()
    {
        // Arrange
        var settings = new VerifySettings();
        settings.DontScrubGuids();

        using var client = Fixture.CreateClient();

        // Act
        string actual = await client.GetStringAsync("/openapi/api.json", CancellationToken);

        // Assert
        await VerifyJson(actual, settings);
    }
}
