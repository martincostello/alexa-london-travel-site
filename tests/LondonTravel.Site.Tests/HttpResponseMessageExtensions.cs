// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Mime;
using System.Text.Json;

namespace MartinCostello.LondonTravel.Site
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<JsonDocument> ReadAsJsonDocumentAsync(this HttpResponseMessage response)
        {
            response.Content.ShouldNotBeNull();
            response.Content!.Headers.ContentType?.MediaType.ShouldBe(MediaTypeNames.Application.Json);
            response.Content.Headers.ContentLength.HasValue.ShouldBeTrue();
            response.Content.Headers.ContentLength.Value.ShouldBeGreaterThan(0);

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }
    }
}
