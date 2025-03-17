// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace MartinCostello.LondonTravel.Site.TagHelpers;

/// <summary>
/// A <see cref="ITagHelper"/> implementation targeting &lt;link&gt;
/// elements that supports inlining styles for local CSS files.
/// </summary>
[HtmlTargetElement("link", Attributes = InlineAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement("link", Attributes = MinifyInlinedAttributeName, TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement("link", Attributes = NonceAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class InlineStyleTagHelper(
    IWebHostEnvironment hostingEnvironment,
    TagHelperMemoryCacheProvider cacheProvider,
    IFileVersionProvider fileVersionProvider,
    HtmlEncoder htmlEncoder,
    JavaScriptEncoder javaScriptEncoder,
    IUrlHelperFactory urlHelperFactory) : LinkTagHelper(hostingEnvironment, cacheProvider, fileVersionProvider, htmlEncoder, javaScriptEncoder, urlHelperFactory)
{
    /// <summary>
    /// The name of the <c>asp-inline</c> attribute.
    /// </summary>
    private const string InlineAttributeName = "asp-inline";

    /// <summary>
    /// The name of the <c>asp-inline</c> attribute.
    /// </summary>
    private const string MinifyInlinedAttributeName = "asp-minify-inlined";

    /// <summary>
    /// The name of the <c>asp-csp-nonce</c> attribute.
    /// </summary>
    private const string NonceAttributeName = "asp-csp-nonce";

    /// <summary>
    /// Gets or sets a value indicating whether CSS should be inlined.
    /// </summary>
    [HtmlAttributeName(InlineAttributeName)]
    public bool? Inline { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether inlined CSS should be minified.
    /// </summary>
    [HtmlAttributeName(MinifyInlinedAttributeName)]
    public bool? MinifyInlined { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        bool shouldProcess =
            Inline == true &&
            string.Equals(context.AllAttributes["rel"]?.Value?.ToString(), "stylesheet", StringComparison.OrdinalIgnoreCase);

        if (!shouldProcess)
        {
            // Not enabled or not a stylesheet
            await base.ProcessAsync(context, output);
            return;
        }

        string? filePath = (context.AllAttributes["href"].Value as string)?.TrimStart('~');
        var fileInfo = HostingEnvironment.WebRootFileProvider.GetFileInfo(filePath!);

        if (!fileInfo.Exists || filePath == null || fileInfo.PhysicalPath is null)
        {
            // Not a local file
            await base.ProcessAsync(context, output);
            return;
        }

        string cacheKey = $"inline-css-{fileInfo.PhysicalPath}-{MinifyInlined == true}";

        if (!Cache.TryGetValue(cacheKey, out string? css))
        {
            using (var stream = File.OpenRead(fileInfo.PhysicalPath))
            {
                using var reader = new StreamReader(stream);
                css = await reader.ReadToEndAsync();
            }

            if (MinifyInlined == true)
            {
                css = MinifyCss(css);
            }

            css = FixSourceMapPath(css, filePath);

            var options = new MemoryCacheEntryOptions()
            {
                Size = css.Length,
            };

            Cache.Set(cacheKey, css, options);
        }

        output.Content.SetHtmlContent(css);

        output.Attributes.Clear();

        if (context.AllAttributes.TryGetAttribute(NonceAttributeName, out var nonceAttribute))
        {
            output.Attributes.Add("nonce", nonceAttribute.Value.ToString());
        }

        output.TagName = "style";
        output.TagMode = TagMode.StartTagAndEndTag;
    }

    /// <summary>
    /// Naively minify the CSS in the specified string.
    /// </summary>
    /// <param name="css">A string containing CSS to minfiy.</param>
    /// <returns>
    /// A string containing the minified representation of <paramref name="css"/>.
    /// </returns>
    private static string MinifyCss(string css)
    {
        // Remove all blank lines, trim space between line contents and turn into a single line
        string[] lines = css.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        string minified = string.Join(string.Empty, lines.Select((p) => p.Trim()));

        var builder = new StringBuilder(minified);

        for (int i = 0; i < builder.Length - 1; i++)
        {
            char ch = builder[i];

            // Remove spaces before element starts, such as: "body {...}" => "body{...}"
            // Remove spaces after delimited lists, such as: "font: a, b, 'c d'" => "font: a,b,'c d'"
            // Remove spaces after value starts, such as: "resize: none;" => "resize:none;"
            if (ch == '{')
            {
                int previous = i - 1;

                if (builder[previous] == ' ')
                {
                    builder.Remove(previous, 1);
                }
            }
            else if (ch is ',' or ':')
            {
                int next = i + 1;

                if (builder[next] == ' ')
                {
                    builder.Remove(next, 1);
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Attempts to fix the source map path for the specified CSS.
    /// </summary>
    /// <param name="css">The CSS that may contain a source map path.</param>
    /// <param name="filePath">The path of the file containing the CSS.</param>
    /// <returns>
    /// The CSS that may have had the source map path fixed.
    /// </returns>
    /// <remarks>
    /// Rendering the CSS inline with an embedded source map path in a comment
    /// may result in 404 errors from trying to fetch the map file with a relative
    /// path to the document where the CSS is inlined, which is unlikely to match
    /// the relative path from the original CSS file that the map file has.
    /// </remarks>
    private string FixSourceMapPath(string css, string filePath)
    {
        // Is there a map file?
        string mapFilePath = $"{filePath}.map";
        var fileInfo = HostingEnvironment.WebRootFileProvider.GetFileInfo(mapFilePath);

        if (fileInfo.Exists)
        {
            const string SourceMapPreamble = "sourceMappingURL=";

            int startIndex = css.IndexOf(SourceMapPreamble, StringComparison.OrdinalIgnoreCase);

            if (startIndex > -1)
            {
                startIndex += SourceMapPreamble.Length;

                int endIndex = css.IndexOf('*', startIndex);

                if (endIndex > -1)
                {
                    css = css.Remove(startIndex, endIndex - startIndex);
                    css = css.Insert(startIndex, mapFilePath);
                }
            }
        }

        return css;
    }
}
