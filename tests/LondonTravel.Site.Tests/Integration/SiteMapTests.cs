// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Mime;
using System.Xml;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class containing tests for the site map.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SiteMapTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
[Collection(TestServerCollection.Name)]
public class SiteMapTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Site_Map_Locations_Are_Valid()
    {
        XmlNodeList? locations;

        // Arrange
        using (var client = Fixture.CreateClient())
        {
            // Act
            using var response = await client.GetAsync("sitemap.xml");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content!.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Text.Xml);

            string xml = await response.Content.ReadAsStringAsync();

            xml.ShouldNotBeNullOrWhiteSpace();
            locations = GetSitemapLocations(xml);
        }

        // Assert
        locations.ShouldNotBeNull();
        locations!.Count.ShouldBeGreaterThan(0);

        foreach (XmlNode? location in locations)
        {
            location.ShouldNotBeNull();

            string url = location!.InnerText;

            url.ShouldNotBeNullOrWhiteSpace();
            Uri.TryCreate(url, UriKind.Absolute, out Uri? uri).ShouldBeTrue();

            uri!.Scheme.ShouldBe("https");
            uri.Port.ShouldBe(443);
            uri.Host.ShouldBe("londontravel.martincostello.com");
            uri.AbsolutePath.ShouldEndWith("/");

            using var client = Fixture.CreateClient();
            using var response = await client.GetAsync(uri.PathAndQuery);

            response.StatusCode.ShouldBe(HttpStatusCode.OK, $"Failed to get {uri.PathAndQuery}. {await response.Content!.ReadAsStringAsync()}");
            response.Content.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Text.Html);
            response.Content.Headers.ContentLength.ShouldNotBeNull();
            response.Content.Headers.ContentLength!.Value.ShouldBeGreaterThan(0);
        }
    }

    private static XmlNodeList? GetSitemapLocations(string xml)
    {
        string prefix = "ns";
        string uri = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var sitemap = new XmlDocument();
        sitemap.LoadXml(xml);

        var nsmgr = new XmlNamespaceManager(sitemap.NameTable);
        nsmgr.AddNamespace(prefix, uri);

        string xpath = string.Format(CultureInfo.InvariantCulture, "/{0}:urlset/{0}:url/{0}:loc", prefix);
        return sitemap.SelectNodes(xpath, nsmgr);
    }
}
