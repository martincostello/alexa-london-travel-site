// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Identity;
    using Microsoft.AspNetCore.Identity;
    using Newtonsoft.Json.Linq;
    using Xunit;

    /// <summary>
    /// A class containing tests for user accounts.
    /// </summary>
    public class AccountTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        public AccountTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "Needs to be conditionally enabled based on the Azure Cosmos Document DB emulator running.")]
        public async Task Can_Perform_Operations_On_Users_And_Get_Preferences_From_Api()
        {
            // Arrange
            var cancellationToken = default(CancellationToken);
            var emailAddress = $"some.user.{Guid.NewGuid()}@some.domain.com";

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

            string accessToken = Controllers.AlexaController.GenerateAccessToken();
            string[] favoriteLines = new[] { "district", "northern" };
            string userId;

            using (IUserStore<LondonTravelUser> store = GetUserStore())
            {
                // Act
                IdentityResult createResult = await store.CreateAsync(user, cancellationToken);

                // Assert
                Assert.NotNull(createResult);
                Assert.True(createResult.Succeeded);
                Assert.NotEmpty(user.Id);

                // Arrange
                userId = user.Id;

                // Act
                LondonTravelUser actual = await store.FindByIdAsync(userId, cancellationToken);

                // Assert
                Assert.NotNull(actual);
                Assert.Equal(userId, actual.Id);
                Assert.Null(actual.AlexaToken);
                Assert.Equal(user.CreatedAt, actual.CreatedAt);
                Assert.Equal(user.Email, actual.Email);
                Assert.Equal(false, actual.EmailConfirmed);
                Assert.NotEmpty(actual.ETag);
                Assert.Equal(Array.Empty<string>(), actual.FavoriteLines);
                Assert.Equal(user.GivenName, actual.GivenName);
                Assert.Equal(Array.Empty<LondonTravelLoginInfo>(), actual.Logins);
                Assert.Equal(user.Surname, actual.Surname);
                Assert.Equal(user.UserName, actual.UserName);

                // Arrange
                string etag = actual.ETag;

                actual.AlexaToken = accessToken;
                actual.FavoriteLines = favoriteLines;

                // Act
                IdentityResult updateResult = await store.UpdateAsync(actual, cancellationToken);

                // Assert
                Assert.NotNull(updateResult);
                Assert.True(updateResult.Succeeded);

                // Act
                actual = await store.FindByNameAsync(emailAddress, cancellationToken);

                // Assert
                Assert.NotNull(actual);
                Assert.Equal(userId, actual.Id);
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
                using (var response = await Fixture.Client.SendAsync(message, cancellationToken))
                {
                    // Assert
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    string json = await response.Content.ReadAsStringAsync();
                    dynamic preferences = JObject.Parse(json);

                    Assert.Equal(userId, (string)preferences.userId);
                    Assert.Equal(favoriteLines, preferences.favoriteLines.ToObject<string[]>() as IList<string>);
                }
            }

            // Arrange
            using (IUserStore<LondonTravelUser> store = GetUserStore())
            {
                // Act
                IdentityResult updateResult = await store.DeleteAsync(new LondonTravelUser() { Id = userId }, cancellationToken);

                // Assert
                Assert.NotNull(updateResult);
                Assert.True(updateResult.Succeeded);
            }

            // Arrange
            using (var message = new HttpRequestMessage(HttpMethod.Get, "api/preferences"))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                // Act
                using (var response = await Fixture.Client.SendAsync(message, cancellationToken))
                {
                    // Assert
                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                }
            }
        }

        /// <summary>
        /// Returns a user store for the application.
        /// </summary>
        /// <returns>
        /// The <see cref="IUserStore{LondonTravelUser}"/> to use for the application.
        /// </returns>
        protected IUserStore<LondonTravelUser> GetUserStore()
        {
            return Fixture.Server.Host.Services.GetService(typeof(IUserStore<LondonTravelUser>)) as IUserStore<LondonTravelUser>;
        }
    }
}
