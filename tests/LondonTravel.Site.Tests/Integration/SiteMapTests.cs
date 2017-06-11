// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// A class containing tests for the site map.
    /// </summary>
    public class SiteMapTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteMapTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        public SiteMapTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task Site_Map_Locations_Are_Valid()
        {
            XmlNodeList locations;

            // Act
            using (var response = await Fixture.Client.GetAsync("sitemap.xml"))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                string xml = await response.Content.ReadAsStringAsync();
                locations = GetSitemapLocations(xml);
            }

            // Assert
            locations.ShouldNotBeNull();
            locations.Count.ShouldBeGreaterThan(0);

            foreach (XmlNode location in locations)
            {
                string url = location.InnerText;
                Assert.True(Uri.TryCreate(url, UriKind.Absolute, out Uri uri));

                uri.Scheme.ShouldBe("https");
                uri.Port.ShouldBe(443);
                uri.Host.ShouldBe("londontravel.martincostello.com");
                uri.AbsolutePath.ShouldEndWith("/");

                using (var response = await Fixture.Client.GetAsync(uri.PathAndQuery))
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            }
        }

        private static XmlNodeList GetSitemapLocations(string xml)
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
}
