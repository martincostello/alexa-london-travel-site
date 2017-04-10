// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Options;
    using Shouldly;
    using Telemetry;
    using Xunit;

    /// <summary>
    /// A class containing tests for the <see cref="AlexaController"/> class. This class cannot be inherited.
    /// </summary>
    public static class AlexaControllerTests
    {
        [Fact]
        public static void Access_Tokens_Are_Randomly_Generated()
        {
            // Arrange
            var tokens = new List<string>();

            // Act
            for (int i = 0; i < 512; i++)
            {
                tokens.Add(AlexaController.GenerateAccessToken());
            }

            // Assert
            tokens.Distinct().Count().ShouldBe(tokens.Count);
            tokens.ShouldAllBe((p) => p.Length >= 64);
        }

        [Fact]
        public static async Task AuthorizeSkill_Returns_Not_Found_If_Account_Linking_Disabled()
        {
            // Arrange
            string state = "Some State";
            string clientId = "SomeClientId";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            SiteOptions options = CreateValidSiteOptions();
            options.Alexa.IsLinkingEnabled = false;

            using (var target = CreateTarget(options: options))
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                actual.ShouldNotBeNull();
                actual.ShouldBeOfType<NotFoundResult>();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public static async Task AuthorizeSkill_Returns_Correct_Location_If_Client_Id_Is_Invalid(string clientId)
        {
            // Arrange
            string state = "Some State";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=invalid_request");
            }
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("some-invalid-value")]
        public static async Task AuthorizeSkill_Returns_Correct_Location_If_Client_Id_Is_Unauthorized(string clientId)
        {
            // Arrange
            string state = "Some State";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=unauthorized_client");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public static async Task AuthorizeSkill_Returns_Correct_Location_If_Response_Type_Is_Invalid(string responseType)
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=invalid_request");
            }
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("code")]
        public static async Task AuthorizeSkill_Returns_Correct_Location_If_Response_Type_Is_Not_Supported(string responseType)
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=unsupported_response_type");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("/local")]
        [InlineData("http://alexa.amazon.com/alexa-london-travel")]
        [InlineData("http://alexa.amazon.com/alexa-london-travel/?foo=baz")]
        [InlineData("https://alexa.amazon.com/alexa-london-travel/unknown")]
        [InlineData("https://bbc.co.uk")]
        public static async Task AuthorizeSkill_Returns_Bad_Request_If_Redirect_Uri_Is_Invalid(string redirectUrl)
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            string responseType = "token";
            Uri redirectUri = redirectUrl == null ? null : new Uri(redirectUrl, UriKind.RelativeOrAbsolute);

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                actual.ShouldNotBeNull();
                actual.ShouldBeOfType<BadRequestResult>();
            }
        }

        [Fact]
        public static async Task AuthorizeSkill_Returns_Error_If_User_Not_Found()
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            using (var target = CreateTarget())
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=server_error");
            }
        }

        [Fact]
        public static async Task AuthorizeSkill_Returns_Error_If_Update_Fails()
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            var user = new LondonTravelUser();
            var result = IdentityResult.Failed(new IdentityError() { Code = "Error", Description = "Problem" });

            var userManager = CreateUserManager(user, result);

            using (var target = CreateTarget(userManager))
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                AssertRedirect(actual, "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=server_error");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("My Existing Token")]
        public static async Task AuthorizeSkill_Returns_Correct_Redirect_Url_If_Token_Created_Or_Updated(string alexaToken)
        {
            // Arrange
            string state = "Some State";
            string clientId = "my-client-id";
            string responseType = "token";
            Uri redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

            var user = new LondonTravelUser()
            {
                AlexaToken = alexaToken,
            };

            var result = IdentityResult.Success;

            var userManager = CreateUserManager(user, result);

            using (var target = CreateTarget(userManager))
            {
                // Act
                IActionResult actual = await target.AuthorizeSkill(state, clientId, responseType, redirectUri);

                // Assert
                actual.ShouldNotBeNull();

                var viewResult = actual.ShouldBeOfType<RedirectResult>();
                viewResult.Permanent.ShouldBeFalse();

                viewResult.Url.ShouldNotBeNullOrWhiteSpace();
                viewResult.Url.ShouldStartWith("https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&access_token=");
                viewResult.Url.ShouldEndWith("&token_type=Bearer");

                if (alexaToken != null)
                {
                    viewResult.Url.ShouldNotContain(alexaToken);
                    viewResult.Url.ShouldNotContain(Uri.EscapeUriString(alexaToken));
                }

                user.AlexaToken.ShouldNotBe(alexaToken);
                user.AlexaToken.Length.ShouldBeGreaterThanOrEqualTo(64);
            }
        }

        private static RedirectResult AssertRedirect(IActionResult actual, string url = null)
        {
            actual.ShouldNotBeNull();

            var result = actual.ShouldBeOfType<RedirectResult>();

            result.Permanent.ShouldBeFalse();

            if (url != null)
            {
                result.Url.ShouldBe(url);
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="ApiController"/> using mock dependencies.
        /// </summary>
        /// <param name="userManager">An optional instance of <see cref="UserManager{TUser}"/>.</param>
        /// <param name="options">An optional instance of <see cref="SiteOptions"/>.</param>
        /// <returns>
        /// The created instance of <see cref=""/>.
        /// </returns>
        private static AlexaController CreateTarget(
            UserManager<LondonTravelUser> userManager = null,
            SiteOptions options = null)
        {
            var httpContext = new Mock<HttpContext>();

            httpContext
                .Setup((p) => p.User)
                .Returns(new ClaimsPrincipal());

            var actionContext = new ActionContext()
            {
                ActionDescriptor = new ControllerActionDescriptor(),
                HttpContext = httpContext.Object,
                RouteData = new RouteData(),
            };

            var controllerContext = new ControllerContext(actionContext);

            return new AlexaController(userManager ?? CreateUserManager(), Mock.Of<ISiteTelemetry>(), options ?? CreateValidSiteOptions(), Mock.Of<ILogger<AlexaController>>())
            {
                ControllerContext = controllerContext,
            };
        }

        /// <summary>
        /// Creates an instance of <see cref="UserManager{TUser}"/>.
        /// </summary>
        /// <param name="user">The optional user to return for calls to get the current user.</param>
        /// <param name="result">The optional identity result return return for calls to update the user.</param>
        /// <returns>
        /// The created instance of <see cref="UserManager{TUser}"/>.
        /// </returns>
        private static UserManager<LondonTravelUser> CreateUserManager(LondonTravelUser user = null, IdentityResult result = null)
        {
            var mock = new Mock<UserManager<LondonTravelUser>>(
                Mock.Of<IUserStore<LondonTravelUser>>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            if (user != null)
            {
                mock.Setup((p) => p.GetUserAsync(It.IsNotNull<ClaimsPrincipal>()))
                    .ReturnsAsync(user);

                if (result != null)
                {
                    mock.Setup((p) => p.UpdateAsync(user))
                        .ReturnsAsync(result);
                }
            }

            return mock.Object;
        }

        /// <summary>
        /// Creates an instance of <see cref="SiteOptions"/> that is valid for use with Alexa.
        /// </summary>
        /// <returns>
        /// The created instance of <see cref="SiteOptions"/>.
        /// </returns>
        private static SiteOptions CreateValidSiteOptions()
        {
            return new SiteOptions()
            {
                Alexa = new AlexaOptions()
                {
                    ClientId = "my-client-id",
                    IsLinkingEnabled = true,
                    RedirectUrls = new[]
                    {
                        "https://alexa.amazon.com/alexa-london-travel?foo=bar",
                        "https://alexa.amazon.com/alexa-london-travel/link",
                    },
                },
            };
        }
    }
}
