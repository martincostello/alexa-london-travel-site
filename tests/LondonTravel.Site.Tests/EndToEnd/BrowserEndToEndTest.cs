// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.EndToEnd;

/// <summary>
/// The base class for end-to-end tests.
/// </summary>
[Collection<WebsiteCollection>]
[Trait("Category", "EndToEnd")]
public abstract class BrowserEndToEndTest : BrowserTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserEndToEndTest"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The test output helper to use.</param>
    protected BrowserEndToEndTest(WebsiteFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Fixture = fixture;
        CaptureTrace = false; // Disabled due to use of secrets for credentials
    }

    /// <summary>
    /// Gets the website fixture.
    /// </summary>
    protected WebsiteFixture Fixture { get; }

    /// <inheritdoc/>
    protected override Uri ServerAddress => Fixture.ServerAddress;
}
