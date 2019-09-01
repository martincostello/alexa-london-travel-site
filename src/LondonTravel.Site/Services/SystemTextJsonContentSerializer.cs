// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Refit;

    /// <summary>
    /// A class representing an implementation of <see cref="IContentSerializer"/>
    /// for the new System.Text.Json serializer. This class cannot be inherited.
    /// </summary>
    public sealed class SystemTextJsonContentSerializer : IContentSerializer
    {
        private static readonly MediaTypeHeaderValue _jsonMediaType =
            new MediaTypeHeaderValue(MediaTypeNames.Application.Json) { CharSet = Encoding.UTF8.WebName };

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonContentSerializer"/> class.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/> to use.</param>
        public SystemTextJsonContentSerializer(IOptions<JsonOptions> options)
        {
            SerializerOptions = options.Value.JsonSerializerOptions;
        }

        private JsonSerializerOptions SerializerOptions { get; }

        /// <inheritdoc/>
        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            using var utf8Json = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(utf8Json, SerializerOptions);
        }

        /// <inheritdoc/>
        public async Task<HttpContent> SerializeAsync<T>(T item)
        {
            var stream = new MemoryStream();

            try
            {
                await JsonSerializer.SerializeAsync(stream, item, SerializerOptions);
                await stream.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);

                var content = new StreamContent(stream);

                content.Headers.ContentType = _jsonMediaType;

                return content;
            }
            catch (Exception)
            {
                await stream.DisposeAsync();
                throw;
            }
        }
    }
}
