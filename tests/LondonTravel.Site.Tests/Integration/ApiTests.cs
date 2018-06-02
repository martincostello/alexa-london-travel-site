// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using Xunit;

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
        public ApiTests(TestServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task Cannot_Get_Preferences_Unauthenticated()
        {
            // Arrange
            using (var client = Fixture.CreateClient())
            {
                // Act
                using (var response = await client.GetAsync(RequestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

                    JObject result = await response.ReadAsObjectAsync();

                    result.Value<string>("requestId").ShouldNotBeNullOrWhiteSpace();
                    result.Value<string>("message").ShouldBe("No access token specified.");
                    result.Value<int>("statusCode").ShouldBe(401);
                    result.Value<JArray>("details").Values<string>().ShouldNotBeNull();
                    result.Value<JArray>("details").Values<string>().ShouldBeEmpty();
                }
            }
        }

        [DocumentDbFact]
        public async Task Cannot_Get_Preferences_With_Invalid_Token()
        {
            // Arrange
            string accessToken = Guid.NewGuid().ToString();

            using (var client = Fixture.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, accessToken);

                // Act
                using (var response = await client.GetAsync(RequestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

                    JObject result = await response.ReadAsObjectAsync();

                    result.Value<string>("requestId").ShouldNotBeNullOrWhiteSpace();
                    result.Value<string>("message").ShouldBe("Unauthorized.");
                    result.Value<int>("statusCode").ShouldBe(401);
                    result.Value<JArray>("details").Values<string>().ShouldNotBeNull();
                    result.Value<JArray>("details").Values<string>().ShouldBeEmpty();
                }
            }
        }

        [DocumentDbFact]
        public async Task Can_Get_Preferences_With_Valid_Token()
        {
            // Arrange
            string token = Environment.GetEnvironmentVariable("ALEXA_ACCESS_TOKEN");

            using (var client = Fixture.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Scheme, token);

                // Act
                using (var response = await client.GetAsync(RequestUri))
                {
                    // Assert
                    response.StatusCode.ShouldBe(HttpStatusCode.OK);

                    JObject result = await response.ReadAsObjectAsync();

                    result.Value<string>("userId").ShouldNotBeNullOrWhiteSpace();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldNotBeNull();
                    result.Value<JArray>("favoriteLines").Values<string>().ShouldNotBeEmpty();
                }
            }
        }
    }
}
