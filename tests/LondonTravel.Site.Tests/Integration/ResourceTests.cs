// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// A class containing tests for loading resources in the website.
    /// </summary>
    public class ResourceTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        public ResourceTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData("/", "text/html")]
        [InlineData("/apple-app-site-association", "application/json")]
        [InlineData("/.well-known/apple-app-site-association", "application/json")]
        [InlineData("/.well-known/assetlinks.json", "application/json")]
        [InlineData("BingSiteAuth.xml", "text/xml")]
        [InlineData("browserconfig.xml", "text/xml")]
        [InlineData("/favicon.ico", "image/x-icon")]
        [InlineData("/humans.txt", "text/plain")]
        [InlineData("/keybase.txt", "text/plain")]
        [InlineData("/manifest.json", "application/json")]
        [InlineData("/robots.txt", "text/plain")]
        [InlineData("/service-worker.js", "application/javascript")]
        [InlineData("/sitemap.xml", "text/xml")]
        public async Task Can_Load_Resource(string requestUri, string contentType)
        {
            using (var response = await Fixture.Client.GetAsync(requestUri))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
            }
        }

        [Fact]
        public async Task Response_Headers_Contains_Expected_Headers()
        {
            string[] expectedHeaders = new[]
            {
                "content-security-policy",
                "X-Content-Type-Options",
                "X-Datacenter",
                "X-Download-Options",
                "X-Frame-Options",
                "X-Instance",
                "X-Request-Id",
                "X-Revision",
                "X-XSS-Protection",
            };

            using (var response = await Fixture.Client.GetAsync("/"))
            {
                foreach (string expected in expectedHeaders)
                {
                    Assert.True(response.Headers.Contains(expected), $"The '{expected}' response header was not found.");
                }
            }
        }
    }
}
