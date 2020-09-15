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
            Fixture.Services!.GetRequiredService<InMemoryDocumentStore>().Clear();
        }

        [Theory]
        [InlineData("amazon", "John")]
        [InlineData("facebook", "John")]
        [InlineData("google", "John")]
        [InlineData("microsoft", "John")]
        [InlineData("twitter", "@JohnSmith")]
        public void Can_Sign_In_And_Out_With_External_Provider(string provider, string expected)
        {
            // Arrange
            AtPage<HomePage>(
                (page) =>
                {
                    page = page
                        .SignIn()
                        .SignInWithProvider(provider);

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.UserName().ShouldBe(expected);

                    // Act
                    page = page.SignOut();

                    // Assert
                    page.IsAuthenticated().ShouldBeFalse();
                });
        }

        [Fact]
        public void Can_Delete_Account()
        {
            // Arrange
            AtPage<HomePage>(
                (homepage) =>
                {
                    ManagePage page = homepage
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
            AtPage<HomePage>(
                (homepage) =>
                {
                    ManagePage page = homepage
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
    }
}
