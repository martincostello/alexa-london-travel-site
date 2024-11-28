// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class containing tests for user preferences in the website.
/// </summary>
public sealed class PreferencesTests : BrowserIntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreferencesTests"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
    public PreferencesTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
        Fixture.Services!.GetRequiredService<InMemoryDocumentStore>().Clear();
    }

    [xRetry.RetryTheory]
    [ClassData(typeof(BrowsersTestData))]
    public async Task Can_Manage_Preferences(string browserType, string? browserChannel)
    {
        // Arrange
        await AtPageAsync<HomePage>(
            browserType,
            browserChannel,
            async (page) =>
            {
                page = await page
                    .SignInAsync()
                    .ThenAsync((p) => p.SignInWithAmazonAsync());

                // Act
                var lines = await page.LinesAsync();

                // Assert
                lines.Count.ShouldBeGreaterThan(2);
                await CountSelectedLinesAsync(lines).ShouldBe(0);

                // Act
                var district = await GetLineAsync(lines, "District");
                var dlr = await GetLineAsync(lines, "DLR");
                var elizabeth = await GetLineAsync(lines, "Elizabeth line");
                var northern = await GetLineAsync(lines, "Northern");
                var windrush = await GetLineAsync(lines, "Windrush");

                district.ShouldNotBeNull();
                dlr.ShouldNotBeNull();
                elizabeth.ShouldNotBeNull();
                northern.ShouldNotBeNull();
                windrush.ShouldNotBeNull();

                await district.ToggleAsync();
                await northern.ToggleAsync();

                page = await page.UpdatePreferencesAsync();

                // Give the UI time to update
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Assert
                lines = await page.LinesAsync();

                lines.Count.ShouldBeGreaterThan(2);

                district = await GetLineAsync(lines, "District");
                northern = await GetLineAsync(lines, "Northern");

                district.ShouldNotBeNull();
                northern.ShouldNotBeNull();

                await district.IsSelectedAsync().ShouldBeTrue();
                await northern.IsSelectedAsync().ShouldBeTrue();

                await CountSelectedLinesAsync(lines).ShouldBe(2);

                // Arrange
                await northern.ToggleAsync();

                // Act
                page = await page.UpdatePreferencesAsync();

                // Give the UI time to update
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Assert
                lines = await page.LinesAsync();

                lines.Count.ShouldBeGreaterThan(2);

                district = await GetLineAsync(lines, "District");
                northern = await GetLineAsync(lines, "Northern");

                district.ShouldNotBeNull();
                northern.ShouldNotBeNull();

                await district.IsSelectedAsync().ShouldBeTrue();
                await northern.IsSelectedAsync().ShouldBeFalse();

                await CountSelectedLinesAsync(lines).ShouldBe(1);
            });

        static async Task<LinePreference?> GetLineAsync(IEnumerable<LinePreference> collection, string name)
        {
            foreach (var line in collection)
            {
                if (string.Equals(await line.NameAsync(), name, StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return null;
        }

        static async Task<int> CountSelectedLinesAsync(IEnumerable<LinePreference> collection)
        {
            int count = 0;

            foreach (var line in collection)
            {
                if (await line.IsSelectedAsync())
                {
                    count++;
                }
            }

            return count;
        }
    }
}
