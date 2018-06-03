// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using Builders;
    using Pages;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A class containing tests for authentication providers in the website.
    /// </summary>
    public sealed class AuthenticationTests : BrowserTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        public AuthenticationTests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public void Can_Sign_In_With_Amazon()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForAmazon());

            // Act and Assert
            SignInWithExternalProvider("amazon");
        }

        [Fact]
        public void Can_Sign_In_With_Facebook()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForFacebook());

            // Act and Assert
            SignInWithExternalProvider("facebook");
        }

        [Fact]
        public void Can_Sign_In_With_Google()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForGoogle());

            // Act and Assert
            SignInWithExternalProvider("google");
        }

        [Fact]
        public void Can_Sign_In_With_Microsoft()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForMicrosoft());

            // Act and Assert
            SignInWithExternalProvider("microsoft");
        }

        [Fact]
        public void Can_Sign_In_With_Twitter()
        {
            // Arrange
            ConfigureExternalProvider(
                (p) =>
                {
                    p.Tokens().WithAccessToken("twitter-oath-token");
                    p.ForTwitter();
                });

            // Act and Assert
            SignInWithExternalProvider("twitter", "@JohnSmith");
        }

        private void SignInWithExternalProvider(string name, string expectedUserName = "John")
        {
            // Act
            WithNavigator(
                (navigator) =>
                {
                    HomePage page = navigator.GoToRoot()
                        .SignIn()
                        .SignInWithProvider(name);

                    // Assert
                    page.UserName().ShouldBe(expectedUserName);
                });
        }

        private void ConfigureExternalProvider(Action<AuthenticationInterceptionBuilder> configure)
        {
            configure(new AuthenticationInterceptionBuilder(Fixture.Interceptor));
        }
    }
}
