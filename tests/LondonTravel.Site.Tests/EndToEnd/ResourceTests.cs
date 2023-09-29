// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Mime;

namespace MartinCostello.LondonTravel.Site.EndToEnd;

[Collection(WebsiteCollection.Name)]
[Trait("Category", "EndToEnd")]
public class ResourceTests(WebsiteFixture fixture)
{
    [SkippableTheory]
    [InlineData("/", MediaTypeNames.Text.Html)]
    [InlineData("/.well-known/apple-app-site-association", MediaTypeNames.Application.Json)]
    [InlineData("/.well-known/assetlinks.json", MediaTypeNames.Application.Json)]
    [InlineData("/.well-known/security.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/account/register/", MediaTypeNames.Text.Html)]
    [InlineData("/account/sign-in/", MediaTypeNames.Text.Html)]
    [InlineData("/api/", MediaTypeNames.Text.Html)]
    [InlineData("/apple-app-site-association", MediaTypeNames.Application.Json)]
    [InlineData("/assets/css/main.css", "text/css")]
    [InlineData("/assets/css/main.css.map", MediaTypeNames.Text.Plain)]
    [InlineData("/assets/js/main.js", "text/javascript")]
    [InlineData("/assets/js/main.js.map", MediaTypeNames.Text.Plain)]
    [InlineData("/BingSiteAuth.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/browserconfig.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/error.html", MediaTypeNames.Text.Html)]
    [InlineData("/favicon.ico", "image/x-icon")]
    [InlineData("/googled1107923138d0b79.html", MediaTypeNames.Text.Html)]
    [InlineData("/help/", MediaTypeNames.Text.Html)]
    [InlineData("/humans.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/keybase.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/manifest.webmanifest", "application/manifest+json")]
    [InlineData("/pgp-key.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/privacy-policy/", MediaTypeNames.Text.Html)]
    [InlineData("/robots.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/security.txt", MediaTypeNames.Text.Plain)]
    [InlineData("/service-worker.js", "text/javascript")]
    [InlineData("/sitemap.xml", MediaTypeNames.Text.Xml)]
    [InlineData("/swagger/api/swagger.json", MediaTypeNames.Application.Json)]
    [InlineData("/technology/", MediaTypeNames.Text.Html)]
    [InlineData("/terms-of-service/", MediaTypeNames.Text.Html)]
    public async Task Can_Load_Resource(string requestUri, string contentType)
    {
        // Arrange
        using var client = fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"Failed to get {requestUri}. {await response.Content!.ReadAsStringAsync()}");
        response.Content.Headers.ContentType?.MediaType.ShouldBe(contentType);
        response.Content.Headers.ContentLength.ShouldNotBeNull();
        response.Content.Headers.ContentLength.ShouldNotBe(0);
    }

    [SkippableFact]
    public async Task Response_Headers_Contains_Expected_Headers()
    {
        // Arrange
        string[] expectedHeaders =
        [
            "content-security-policy",
            "content-security-policy-report-only",
            "NEL",
            "Permissions-Policy",
            "Referrer-Policy",
            "Report-To",
            "X-Content-Type-Options",
            "X-CSP-Nonce",
            "X-Datacenter",
            "X-Download-Options",
            "X-Frame-Options",
            "X-Instance",
            "X-Request-Id",
            "X-Revision",
            "X-XSS-Protection",
        ];

        // Act
        using var client = fixture.CreateClient();
        var response = await client.GetAsync("/");

        // Assert
        foreach (string expected in expectedHeaders)
        {
            response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
        }
    }
}
