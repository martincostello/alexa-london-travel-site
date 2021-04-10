// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.EndToEnd
{
    using System;
    using MartinCostello.LondonTravel.Site.Pages;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class SocialLoginTests : BrowserEndToEndTest
    {
        public SocialLoginTests(WebsiteFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [SkippableFact]
        public void Can_Sign_In_With_Google()
        {
            SignInWithSocialProvider(
                "Google",
                (driver, userName, password) =>
                {
                    var userNameSelector = By.CssSelector("input[type=email]");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    SignIn(
                        driver,
                        userNameSelector,
                        passwordSelector,
                        (userName + Keys.Enter, password));
                });
        }

        [SkippableFact]
        public void Can_Sign_In_With_Microsoft_Account()
        {
            SignInWithSocialProvider(
                "Microsoft",
                (driver, userName, password) =>
                {
                    var userNameSelector = By.CssSelector("input[type=email]");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    SignIn(
                        driver,
                        userNameSelector,
                        passwordSelector,
                        (userName + Keys.Enter, password));
                });
        }

        [SkippableFact]
        public void Can_Sign_In_With_Twitter()
        {
            Skip.IfNot(
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")),
                "Sign-in blocked when run in GitHub Actions.");

            SignInWithSocialProvider(
                "Twitter",
                (driver, userName, password) =>
                {
                    var userNameSelector = By.Id("username_or_email");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    SignIn(
                        driver,
                        userNameSelector,
                        passwordSelector,
                        (userName, password));
                });
        }

        private void SignInWithSocialProvider(string providerName, Action<IWebDriver, string, string> signIn)
        {
            string? userName = Environment.GetEnvironmentVariable($"WEBSITE_USER_{providerName.ToUpperInvariant()}_USERNAME");
            string? password = Environment.GetEnvironmentVariable($"WEBSITE_USER_{providerName.ToUpperInvariant()}_PASSWORD");

            Skip.If(string.IsNullOrWhiteSpace(userName), $"No {providerName} username is configured.");
            Skip.If(string.IsNullOrWhiteSpace(password), $"No {providerName} password is configured.");

            // Arrange
            AtPage<HomePage>(
                (page) =>
                {
#pragma warning disable CA1308
                    page = page.SignIn()
                               .SignInWithProvider(providerName.ToLowerInvariant());
#pragma warning restore CA1308

                    signIn(page.Navigator.Driver, userName, password);

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.UserName().ShouldNotBeNullOrWhiteSpace();

                    // Act
                    page = page.SignOut();

                    // Assert
                    page.IsAuthenticated().ShouldBeFalse();
                });
        }

        private void SignIn(
            IWebDriver driver,
            By userNameSelector,
            By passwordSelector,
            (string userName, string password) credentials)
        {
            var userName = driver.FindElement(userNameSelector);
            userName.SendKeys(credentials.userName);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
            wait.Until((p) => p.FindElement(passwordSelector).Displayed);

            var password = driver.FindElement(passwordSelector);
            password.SendKeys(credentials.password + Keys.Enter);

            wait.Until((p) => p.Url.StartsWith(ServerAddress.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
