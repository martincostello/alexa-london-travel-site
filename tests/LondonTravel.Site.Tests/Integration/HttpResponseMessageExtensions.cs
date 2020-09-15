// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System.Net.Http;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Shouldly;

    public static class HttpResponseMessageExtensions
    {
        public static async Task<JsonDocument> ReadAsJsonDocumentAsync(this HttpResponseMessage response)
        {
            response.Content.ShouldNotBeNull();
            response.Content!.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
            response.Content.Headers.ContentLength.ShouldNotBeNull();
            response.Content.Headers.ContentLength.ShouldNotBe(0);

            string json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
    }
}
