// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            SignInWithExternalProviderAndSignOut("amazon");
        }

        [Fact]
        public void Can_Sign_In_With_Facebook()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForFacebook());

            // Act and Assert
            SignInWithExternalProviderAndSignOut("facebook");
        }

        [Fact]
        public void Can_Sign_In_With_Google()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForGoogle());

            // Act and Assert
            SignInWithExternalProviderAndSignOut("google");
        }

        [Fact]
        public void Can_Sign_In_With_Microsoft()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForMicrosoft());

            // Act and Assert
            SignInWithExternalProviderAndSignOut("microsoft");
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
            SignInWithExternalProviderAndSignOut("twitter", "@JohnSmith");
        }

        [Fact]
        public void Can_Delete_Account()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForAmazon());

            WithNavigator(
                (navigator) =>
                {
                    ManagePage page = navigator.GoToRoot()
                        .SignIn()
                        .SignInWithAmazon()
                        .Manage();

                    // Act
                    page.DeleteAccount()
                        .Close();

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();

                    // Act
                    page.DeleteAccount()
                        .Confirm();

                    // Assert
                    page.IsAuthenticated().ShouldBeFalse();
                });
        }

        [Fact]
        public void Can_Link_Accounts()
        {
            // Arrange
            ConfigureExternalProvider((p) => p.ForAmazon().ForGoogle());

            WithNavigator(
                (navigator) =>
                {
                    ManagePage page = navigator.GoToRoot()
                        .SignIn()
                        .SignInWithAmazon()
                        .Manage();

                    // Assert
                    IReadOnlyCollection<LinkedAccount> accounts = page.LinkedAccounts();

                    accounts.Count.ShouldBe(1);
                    accounts.First().Name().ShouldBe("Amazon");

                    // Act
                    page = page.SignInWithGoogle();

                    // Assert
                    accounts = page.LinkedAccounts();

                    accounts.Count.ShouldBe(2);
                    accounts.First().Name().ShouldBe("Amazon");
                    accounts.Last().Name().ShouldBe("Google");

                    // Act
                    page = accounts.First().Remove();

                    // Assert
                    accounts = page.LinkedAccounts();

                    accounts.Count.ShouldBe(1);
                    accounts.First().Name().ShouldBe("Google");
                });
        }

        private void SignInWithExternalProviderAndSignOut(string name, string expectedUserName = "John")
        {
            WithNavigator(
                (navigator) =>
                {
                    // Act
                    HomePage page = navigator.GoToRoot()
                        .SignIn()
                        .SignInWithProvider(name);

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.UserName().ShouldBe(expectedUserName);

                    // Act
                    page = page.SignOut();

                    // Assert
                    page.IsAuthenticated().ShouldBeFalse();
                });
        }

        private void ConfigureExternalProvider(Action<AuthenticationInterceptionBuilder> configure)
        {
            configure(new AuthenticationInterceptionBuilder(Fixture.Interceptor));
        }
    }
}
