// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Pages;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A class containing tests for user preferences in the website.
    /// </summary>
    public sealed class PreferencesTests : BrowserTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        public PreferencesTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
            Fixture.Services.GetRequiredService<InMemoryDocumentStore>().Clear();
        }

        [Fact]
        public void Can_Manage_Preferences()
        {
            // Arrange
            AtPage<HomePage>(
                (page) =>
                {
                    page = page
                        .SignIn()
                        .SignInWithAmazon();

                    // Act
                    IReadOnlyCollection<LinePreference> lines = page.Lines();

                    // Assert
                    lines.Count.ShouldBeGreaterThan(2);
                    lines.Count((p) => p.IsSelected()).ShouldBe(0);

                    // Act
                    lines.First((p) => p.Name() == "District").Toggle();
                    lines.First((p) => p.Name() == "Northern").Toggle();
                    page = page.UpdatePreferences();

                    // Assert
                    lines = page.Lines();

                    lines.Count.ShouldBeGreaterThan(2);
                    lines.First((p) => p.Name() == "District").IsSelected().ShouldBeTrue();
                    lines.First((p) => p.Name() == "Northern").IsSelected().ShouldBeTrue();
                    lines.Count((p) => p.IsSelected()).ShouldBe(2);
                });
        }
    }
}
