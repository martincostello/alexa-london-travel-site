// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http.Headers;

namespace MartinCostello.LondonTravel.Site.EndToEnd;

public class WebsiteFixture
{
    private const string WebsiteUrl = "WEBSITE_URL";

    public WebsiteFixture()
    {
        string url = Environment.GetEnvironmentVariable(WebsiteUrl) ?? string.Empty;

        if (Uri.TryCreate(url, UriKind.Absolute, out var address))
        {
            ServerAddress = address;
        }
    }

    public Uri ServerAddress
    {
        get
        {
            Assert.SkipWhen(field is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");
            return field!;
        }
        set;
    }

    public HttpClient CreateClient()
    {
        var client = new HttpClient()
        {
            BaseAddress = ServerAddress,
        };

        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                "MartinCostello.LondonTravel.Site.Tests",
                "1.0.0+" + GitMetadata.Commit));

        return client;
    }
}
