// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AspNet.Security.OAuth.Amazon;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Shouldly;
    using Xunit;

    public static class OAuthEventsHandlerTests
    {
        [Theory]
        [InlineData("", "/?Message=LinkFailed")]
        [InlineData("?state=foo&error=server_error", "/?Message=LinkFailed")]
        [InlineData("?state=foo&error=access_denied", "/?Message=LinkDenied")]
        [InlineData("?state=foo&error=consent_required", "/?Message=LinkDenied")]
        [InlineData("?state=foo&error_reason=user_denied", "/?Message=LinkDenied")]
        [InlineData("?state=foo&error_description=Access was denied.", "/?Message=LinkDenied")]
        [InlineData("?state=foo&denied=Permissions not granted", "/?Message=LinkDenied")]
        public static async Task Handle_Denied_Permissions(string query, string expected)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString(query);

            var scheme = new AuthenticationScheme("amazon-auth", "Amazon", typeof(AmazonAuthenticationHandler));
            var options = new RemoteAuthenticationOptions();
            var failure = new InvalidOperationException();

            var context = new RemoteFailureContext(httpContext, scheme, options, failure);
            var provider = Guid.NewGuid().ToString();
            var secureDataFormat = Mock.Of<ISecureDataFormat<object>>();
            var logger = Mock.Of<ILogger>();

            // Act
            await OAuthEventsHandler.HandleRemoteFailure(context, provider, secureDataFormat, logger, PropertiesProvider);

            // Assert
            httpContext.Response.GetTypedHeaders().Location.OriginalString.ShouldBe(expected);
        }

        private static IDictionary<string, string?> PropertiesProvider<T>(T value) => new Dictionary<string, string?>();
    }
}
