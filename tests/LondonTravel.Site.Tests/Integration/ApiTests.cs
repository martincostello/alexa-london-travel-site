// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Readers;
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
[Collection(TestServerCollection.Name)]
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
        using var response = await client.GetAsync(RequestUri);

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
        using var response = await client.GetAsync(RequestUri);

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
        using var response = await client.GetAsync(RequestUri);

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
        using var response = await client.GetAsync(RequestUri);

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
    public async Task OpenApi_Documentation_Only_Exposes_Expected_Operations()
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var actual = await client.GetFromJsonAsync<JsonDocument>("/openapi/api.json");

        // Assert
        actual.ShouldNotBeNull();
        actual.RootElement.GetString("openapi").ShouldBe("3.0.0");
        actual.RootElement.GetProperty("info").ValueKind.ShouldBe(JsonValueKind.Object);
        actual.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Count().ShouldBe(2);
        actual.RootElement.GetProperty("paths").EnumerateObject().Count().ShouldBe(1);
        actual.RootElement.GetProperty("security").GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task Schema_Has_No_Validation_Warnings()
    {
        // Arrange
        var ruleSet = ValidationRuleSet.GetDefaultRuleSet();

        // HACK Workaround for https://github.com/microsoft/OpenAPI.NET/issues/1738
        ruleSet.Remove("MediaTypeMismatchedDataType");

        using var client = Fixture.CreateClient();

        // Act
        using var schema = await client.GetStreamAsync("/openapi/api.json");

        // Assert
        var reader = new OpenApiStreamReader();
        var actual = await reader.ReadAsync(schema);

        actual.OpenApiDiagnostic.Errors.ShouldBeEmpty();

        var errors = actual.OpenApiDocument.Validate(ruleSet);
        errors.ShouldBeEmpty();
    }
}
