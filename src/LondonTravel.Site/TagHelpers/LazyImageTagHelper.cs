// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MartinCostello.LondonTravel.Site.TagHelpers;

/// <summary>
/// A <see cref="ITagHelper"/> implementation targeting &lt;lazyimg&gt;
/// elements that supports file versioning and lazy loading of images.
/// </summary>
[HtmlTargetElement("lazyimg", TagStructure = TagStructure.WithoutEndTag)]
public sealed class LazyImageTagHelper(IFileVersionProvider fileVersionProvider, HtmlEncoder htmlEncoder, IUrlHelperFactory urlHelperFactory) : ImageTagHelper(fileVersionProvider, htmlEncoder, urlHelperFactory)
{
    /// <summary>
    /// The name of the <c>class</c> attribute.
    /// </summary>
    private const string ClassAttributeName = "class";

    /// <summary>
    /// The name of the <c>data-original</c> attribute.
    /// </summary>
    private const string DataOriginalAttributeName = "data-original";

    /// <summary>
    /// The name of the <c>src</c> attribute.
    /// </summary>
    private const string SourceAttributeName = "src";

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        // Get the non-lazy image and the current CSS
        string? dataOriginal = output.Attributes[SourceAttributeName].Value.ToString();
        string? css = $"{output.Attributes[ClassAttributeName]?.Value} lazy";

        // Add a placeholder as the src, set the original to be the lazily-loaded
        // image and add the CSS class to get the JavaScript to do the lazy loading.
        output.Attributes.SetAttribute(ClassAttributeName, css);
        output.Attributes.SetAttribute(DataOriginalAttributeName, dataOriginal);
        output.Attributes.SetAttribute(SourceAttributeName, "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        // Ensure the output tag is correct
        output.TagName = "img";
        output.TagMode = TagMode.SelfClosing;
    }
}
