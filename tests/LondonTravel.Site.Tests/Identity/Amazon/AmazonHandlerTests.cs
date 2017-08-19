// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using Xunit;

    public static class AmazonHandlerTests
    {
        [Fact]
        public static async Task Can_Create_Principal_From_Amazon_User_Endpoint()
        {
            // Arrange
            string accessToken = Guid.NewGuid().ToString();

            var data = new
            {
                email = "martin@martincostello.local",
                name = "Martin Costello",
                user_id = "my-user-id",
            };

            var options = new AmazonOptions()
            {
                ClientId = "amazon-client-id",
                ClientSecret = "amazon-client-secret",
            };

            var identity = new ClaimsIdentity();
            var properties = new AuthenticationProperties();
            var tokens = CreateOAuthToken(accessToken);

            AuthenticationTicket actual;

            using (var handler = new AmazonHttpMessageHandler(accessToken, data))
            {
                using (var backchannel = new HttpClient(handler, false))
                {
                    options.Backchannel = backchannel;

                    AmazonHandler target = await CreateTarget(options);

                    // Act
                    actual = await target.CreateAuthenticationTicketAsync(identity, properties, tokens);
                }
            }

            // Assert
            actual.ShouldNotBeNull();

            actual.AuthenticationScheme.ShouldBe(AmazonDefaults.AuthenticationScheme);
            actual.Principal.HasClaim(ClaimTypes.Email, data.email).ShouldBeTrue();
            actual.Principal.HasClaim(ClaimTypes.Name, data.name).ShouldBeTrue();
            actual.Principal.HasClaim(ClaimTypes.NameIdentifier, data.user_id).ShouldBeTrue();
        }

        private static async Task<AmazonHandler> CreateTarget(AmazonOptions options)
        {
            var mock = new Mock<IOptionsMonitor<AmazonOptions>>();

            mock.Setup((p) => p.Get("Amazon"))
                .Returns(options);

            var loggerFactory = new LoggerFactory();
            var urlEncoder = UrlEncoder.Default;
            var clock = new SystemClock();

            var handler = new AmazonHandler(mock.Object, loggerFactory, urlEncoder, clock);

            var scheme = new AuthenticationScheme("Amazon", "Amazon", handler.GetType());
            var context = new DefaultHttpContext();

            await handler.InitializeAsync(scheme, context);

            return handler;
        }

        private static OAuthTokenResponse CreateOAuthToken(string accessToken)
        {
            var tokenResponse = new JObject()
            {
                ["access_token"] = accessToken,
            };

            return OAuthTokenResponse.Success(tokenResponse);
        }

        private sealed class AmazonHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _accessToken;
            private readonly object _content;

            public AmazonHttpMessageHandler(string accessToken, object content)
            {
                _accessToken = accessToken;
                _content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Method != HttpMethod.Get)
                {
                    throw new InvalidOperationException("The HTTP method is incorrect.");
                }

                if (request.RequestUri != new Uri($"https://api.amazon.com/user/profile?access_token={_accessToken}&fields=email,name,user_id"))
                {
                    throw new InvalidOperationException("The user information endpoint URI is incorrect.");
                }

                string json = JsonConvert.SerializeObject(_content);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(json, Encoding.Default, "application/json");

                return Task.FromResult(response);
            }
        }
    }
}
