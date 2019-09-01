// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.TagHelpers
{
    using System;
    using System.Security.Cryptography;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;

    /// <summary>
    /// A <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements to add subresource integrity attributes.
    /// </summary>
    [HtmlTargetElement("script", Attributes = AddSriAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SriScriptTagHelper : LinkTagHelper
    {
        /// <summary>
        /// The name of the <c>asp-add-sri</c> attribute.
        /// </summary>
        private const string AddSriAttributeName = "asp-add-sri";

        /// <summary>
        /// An array containing the <c>~</c> character.
        /// </summary>
        private static readonly char[] Tilde = new[] { '~' };

        public SriScriptTagHelper(IWebHostEnvironment hostingEnvironment, TagHelperMemoryCacheProvider cacheProvider, IFileVersionProvider fileVersionProvider, HtmlEncoder htmlEncoder, JavaScriptEncoder javaScriptEncoder, IUrlHelperFactory urlHelperFactory)
            : base(hostingEnvironment, cacheProvider, fileVersionProvider, htmlEncoder, javaScriptEncoder, urlHelperFactory)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether Subresource Integrity attributes should be added.
        /// </summary>
        [HtmlAttributeName(AddSriAttributeName)]
        public bool? AddSubresourceIntegrity { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (AddSubresourceIntegrity != true)
            {
                await base.ProcessAsync(context, output);
                return;
            }

            string filePath = (context.AllAttributes["src"].Value as string)?.TrimStart(Tilde);
            string type = context.AllAttributes["type"]?.Value?.ToString() as string;

            if (string.IsNullOrEmpty(filePath) ||
                (!string.IsNullOrEmpty(type) && !string.Equals(type, "text/javascript", StringComparison.OrdinalIgnoreCase)))
            {
                // No file or not JavaScript
                await base.ProcessAsync(context, output);
                return;
            }

            IFileInfo fileInfo = HostingEnvironment.WebRootFileProvider.GetFileInfo(filePath);

            if (!fileInfo.Exists)
            {
                // Not a local file
                await base.ProcessAsync(context, output);
                return;
            }

            string cacheKey = $"sri-hash-{fileInfo.PhysicalPath}";

            if (!Cache.TryGetValue(cacheKey, out string hash))
            {
                using (var algorithm = SHA384.Create())
                {
                    using var stream = fileInfo.CreateReadStream();
                    hash = Convert.ToBase64String(algorithm.ComputeHash(stream));
                }

                var options = new MemoryCacheEntryOptions()
                {
                    Size = hash.Length,
                };

                Cache.Set(cacheKey, hash, options);
            }

            output.Attributes.Add("integrity", $"sha384-{hash}");
            output.Attributes.Add("crossorigin", "anonymous");
        }
    }
}
