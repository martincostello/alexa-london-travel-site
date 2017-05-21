// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Models;
    using Moq;
    using Services.Data;
    using Shouldly;
    using Telemetry;
    using Xunit;

    /// <summary>
    /// A class containing tests for the <see cref="ApiController"/> class. This class cannot be inherited.
    /// </summary>
    public static class ApiControllerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public static async Task Preferences_Returns_Unauthorized_Json_If_No_Authorization_Header_Specified(string authorizationHeader)
        {
            // Arrange
            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.GetPreferences(authorizationHeader, default(CancellationToken));

                // Assert
                actual.ShouldNotBeNull();

                var objectResult = actual.ShouldBeOfType<ObjectResult>();

                objectResult.StatusCode.ShouldBe(401);
                objectResult.Value.ShouldNotBeNull();

                var data = objectResult.Value.ShouldBeOfType<ErrorResponse>();

                data.Message.ShouldBe("No access token specified.");
                data.StatusCode.ShouldBe(401);
            }
        }

        [Theory]
        [InlineData("bearer")]
        [InlineData("bearer ")]
        [InlineData("something token")]
        public static async Task Preferences_Returns_Unauthorized_Json_If_Authorization_Header_Is_Invalid(string authorizationHeader)
        {
            // Arrange
            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.GetPreferences(authorizationHeader, default(CancellationToken));

                // Assert
                actual.ShouldNotBeNull();

                var objectResult = actual.ShouldBeOfType<ObjectResult>();

                objectResult.StatusCode.ShouldBe(401);
                objectResult.Value.ShouldNotBeNull();

                var data = objectResult.Value.ShouldBeOfType<ErrorResponse>();

                data.Message.ShouldBe("Unauthorized.");
                data.StatusCode.ShouldBe(401);
            }
        }

        [Fact]
        public static async Task Preferences_Returns_Unauthorized_Json_If_Authorization_Header_Does_Not_Match_A_User()
        {
            // Arrange
            var users = new[]
            {
                new LondonTravelUser() { AlexaToken = null },
                new LondonTravelUser() { AlexaToken = string.Empty },
                new LondonTravelUser() { AlexaToken = "foo" },
                new LondonTravelUser() { AlexaToken = "bar" },
                new LondonTravelUser() { AlexaToken = "bar" },
            };

            string authorizationHeader = "bearer BAR";

            var client = CreateClient(users);

            using (var target = CreateTarget(client))
            {
                // Act
                IActionResult actual = await target.GetPreferences(authorizationHeader, default(CancellationToken));

                // Assert
                actual.ShouldNotBeNull();

                var objectResult = actual.ShouldBeOfType<ObjectResult>();

                objectResult.StatusCode.ShouldBe(401);
                objectResult.Value.ShouldNotBeNull();

                var data = objectResult.Value.ShouldBeOfType<ErrorResponse>();

                data.Message.ShouldBe("Unauthorized.");
                data.StatusCode.ShouldBe(401);
            }
        }

        [Fact]
        public static async Task Preferences_Returns_Unauthorized_Json_If_Authorization_Header_Does_Not_Match_Alexa_Token_Of_Found_User()
        {
            // Arrange
            var users = new[]
            {
                new LondonTravelUser() { AlexaToken = null },
                new LondonTravelUser() { AlexaToken = string.Empty },
                new LondonTravelUser() { AlexaToken = "foo" },
                new LondonTravelUser() { AlexaToken = "bar" },
                new LondonTravelUser() { AlexaToken = "bar" },
            };

            string authorizationHeader = "bearer BAR";

            var client = CreateClient(users);

            using (var target = CreateTarget(client))
            {
                // Act
                IActionResult actual = await target.GetPreferences(authorizationHeader, default(CancellationToken));

                // Assert
                actual.ShouldNotBeNull();

                var objectResult = actual.ShouldBeOfType<ObjectResult>();

                objectResult.StatusCode.ShouldBe(401);
                objectResult.Value.ShouldNotBeNull();

                var data = objectResult.Value.ShouldBeOfType<ErrorResponse>();

                data.Message.ShouldBe("Unauthorized.");
                data.StatusCode.ShouldBe(401);
            }
        }

        [Fact]
        public static async Task Preferences_Returns_Correct_User_Preferences_If_Token_Matches_User()
        {
            // Arrange
            var users = new[]
            {
                new LondonTravelUser() { Id = "1", AlexaToken = null, FavoriteLines = new string[0] },
                new LondonTravelUser() { Id = "2", AlexaToken = string.Empty, FavoriteLines = new string[0] },
                new LondonTravelUser() { Id = "3", AlexaToken = "foo", FavoriteLines = new string[0] },
                new LondonTravelUser() { Id = "4", AlexaToken = "bar", FavoriteLines = new[] { "central", "victoria" } },
                new LondonTravelUser() { Id = "5", AlexaToken = "bar", FavoriteLines = new[] { "circle", "waterloo-city" } },
                new LondonTravelUser() { Id = "6", AlexaToken = "BAR", FavoriteLines = new[] { "district" } },
                new LondonTravelUser() { Id = "7", AlexaToken = "bAr", FavoriteLines = new[] { "district" } },
            };

            string authorizationHeader = "BEARER BAR";

            var client = CreateClient(users);

            using (var target = CreateTarget(client))
            {
                // Act
                IActionResult actual = await target.GetPreferences(authorizationHeader, default(CancellationToken));

                // Assert
                actual.ShouldNotBeNull();

                var objectResult = actual.ShouldBeOfType<OkObjectResult>();

                objectResult.StatusCode.ShouldBe(200);
                objectResult.Value.ShouldNotBeNull();

                var data = objectResult.Value.ShouldBeOfType<PreferencesResponse>();

                data.FavoriteLines.ShouldBe(new[] { "district" });
                data.UserId.ShouldBe("6");
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="ApiController"/> using mock dependencies.
        /// </summary>
        /// <param name="client">An optional instance of <see cref="IDocumentClient"/>.</param>
        /// <returns>
        /// The created instance of <see cref=""/>.
        /// </returns>
        private static ApiController CreateTarget(IDocumentClient client = null)
        {
            var httpRequest = new Mock<HttpRequest>();

            httpRequest.Setup((p) => p.Headers).Returns(new HeaderDictionary());

            var httpContext = new Mock<HttpContext>();

            httpContext.Setup((p) => p.Connection).Returns(Mock.Of<ConnectionInfo>());
            httpContext.Setup((p) => p.Request).Returns(httpRequest.Object);

            var actionContext = new ActionContext()
            {
                ActionDescriptor = new ControllerActionDescriptor(),
                HttpContext = httpContext.Object,
                RouteData = new RouteData(),
            };

            var controllerContext = new ControllerContext(actionContext);

            return new ApiController(client ?? Mock.Of<IDocumentClient>(), Mock.Of<ISiteTelemetry>(), Mock.Of<ILogger<ApiController>>())
            {
                ControllerContext = controllerContext,
            };
        }

        /// <summary>
        /// Creates a mock implementation of <see cref="IDocumentClient"/>.
        /// </summary>
        /// <param name="users">The users to store in the mock implementation.</param>
        /// <returns>
        /// The created mock instance of <see cref="IDocumentClient"/>.
        /// </returns>
        private static IDocumentClient CreateClient(IEnumerable<LondonTravelUser> users)
        {
            Mock<IDocumentClient> mock = new Mock<IDocumentClient>();

            mock.Setup((p) => p.GetAsync(It.IsAny<Expression<Func<LondonTravelUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expression<Func<LondonTravelUser, bool>> a, CancellationToken b) => users.Where(a.Compile()));

            return mock.Object;
        }
    }
}
