// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;

namespace MartinCostello.LondonTravel.Site.Integration
{
    /// <summary>
    /// A class containing tests for the API.
    /// </summary>
    public class ApiTests : IntegrationTest
    {
        private const string Scheme = "bearer";

        private const string RequestUri = "/api/preferences";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        public ApiTests(TestServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task Cannot_Get_Preferences_Unauthenticated()
        {
            // Arrange
            using var client = Fixture.CreateClient();

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

        [Fact]
        public async Task Cannot_Get_Preferences_With_Invalid_Token()
        {
            // Arrange
            string accessToken = Guid.NewGuid().ToString();

            using var client = Fixture.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, accessToken);

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
    }
}
