// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Shouldly;

    public static class HttpResponseMessageExtensions
    {
        public static async Task<JObject> ReadAsObjectAsync(this HttpResponseMessage response)
        {
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
            response.Content.Headers.ContentLength.ShouldNotBeNull();
            response.Content.Headers.ContentLength.ShouldNotBe(0);

            string json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }
    }
}
