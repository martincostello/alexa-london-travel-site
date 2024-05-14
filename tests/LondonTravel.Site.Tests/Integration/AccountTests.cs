// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class containing tests for user accounts.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AccountTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
[Collection(TestServerCollection.Name)]
public class AccountTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Fact]
    public async Task Can_Perform_Operations_On_Users_And_Get_Preferences_From_Api()
    {
        // Arrange
        string? emailAddress = $"some.user.{Guid.NewGuid()}@some.domain.com";

        var user = new LondonTravelUser()
        {
            CreatedAt = DateTime.UtcNow,
            Email = emailAddress,
            EmailNormalized = emailAddress,
            GivenName = "Alexa",
            Surname = "Amazon",
            UserName = emailAddress,
            UserNameNormalized = emailAddress,
        };

        string accessToken = Services.AlexaService.GenerateAccessToken();
        string[] favoriteLines = ["district", "northern"];
        string userId;

        static IUserStore<LondonTravelUser> GetUserStore(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IUserStore<LondonTravelUser>>();

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            using var store = GetUserStore(scope.ServiceProvider);

            // Act
            var createResult = await store.CreateAsync(user, default);

            // Assert
            Assert.NotNull(createResult);
            Assert.True(createResult.Succeeded);
            Assert.NotNull(user.Id);
            Assert.NotEmpty(user.Id);

            // Arrange
            userId = user.Id!;

            // Act
            var actual = await store.FindByIdAsync(userId, default);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(userId, actual!.Id);
            Assert.Null(actual.AlexaToken);
            Assert.Equal(user.CreatedAt, actual.CreatedAt);
            Assert.Equal(user.Email, actual.Email);
            Assert.False(actual.EmailConfirmed);
            Assert.NotNull(actual.ETag);
            Assert.NotEmpty(actual.ETag);
            Assert.Equal([], actual.FavoriteLines);
            Assert.Equal(user.GivenName, actual.GivenName);
            Assert.Equal([], actual.Logins);
            Assert.Equal(user.Surname, actual.Surname);
            Assert.Equal(user.UserName, actual.UserName);

            // Arrange
            string etag = actual.ETag!;

            actual.AlexaToken = accessToken;
            actual.FavoriteLines = favoriteLines;

            // Act
            var updateResult = await store.UpdateAsync(actual, default);

            // Assert
            Assert.NotNull(updateResult);
            Assert.True(updateResult.Succeeded);

            // Act
            actual = await store.FindByNameAsync(emailAddress, default);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(userId, actual!.Id);
            Assert.Equal(emailAddress, actual.Email);
            Assert.NotEqual(etag, actual.ETag);
            Assert.Equal(accessToken, actual.AlexaToken);
            Assert.Equal(favoriteLines, actual.FavoriteLines);
        }

        // Arrange
        using (var message = new HttpRequestMessage(HttpMethod.Get, "api/preferences"))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            // Act
            using var client = Fixture.CreateClient();
            using var response = await client.SendAsync(message);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.ShouldNotBeNull();
            response.Content!.Headers.ShouldNotBeNull();
            response.Content!.Headers!.ContentType.ShouldNotBeNull();
            response.Content.Headers.ContentType!.MediaType.ShouldBe(MediaTypeNames.Application.Json);

            string json = await response.Content.ReadAsStringAsync();
            using var preferences = JsonDocument.Parse(json);

            Assert.Equal(userId, preferences.RootElement.GetString("userId"));
            Assert.Equal(favoriteLines, preferences.RootElement.GetStringArray("favoriteLines"));
        }

        // Arrange
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            using var store = GetUserStore(scope.ServiceProvider);

            // Act
            var updateResult = await store.DeleteAsync(new LondonTravelUser() { Id = userId }, default);

            // Assert
            Assert.NotNull(updateResult);
            Assert.True(updateResult.Succeeded);
        }

        // Arrange
        using (var message = new HttpRequestMessage(HttpMethod.Get, "api/preferences"))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            // Act
            using var client = Fixture.CreateClient();
            using var response = await client.SendAsync(message);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            response.Content.ShouldNotBeNull();
            response.Content!.Headers.ShouldNotBeNull();
            response.Content!.Headers!.ContentType.ShouldNotBeNull();
            response.Content.Headers.ContentType!.MediaType.ShouldBe(MediaTypeNames.Application.Json);
        }
    }
}
