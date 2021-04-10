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
            string? userName = Environment.GetEnvironmentVariable("WEBSITE_USER_GOOGLE_USERNAME");
            string? password = Environment.GetEnvironmentVariable("WEBSITE_USER_GOOGLE_PASSWORD");

            Skip.If(string.IsNullOrWhiteSpace(userName), "No Google username is configured.");
            Skip.If(string.IsNullOrWhiteSpace(password), "No Google password is configured.");

            SignInWithSocialProvider(
                "google",
                (driver) =>
                {
                    var userNameSelector = By.CssSelector("input[type=email]");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    var userNameElement = driver.FindElement(userNameSelector);
                    userNameElement.SendKeys(userName + Keys.Enter);

                    var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    webDriverWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                    webDriverWait.Until((p) => p.FindElement(passwordSelector).Displayed);

                    var passwordElement = driver.FindElement(passwordSelector);
                    passwordElement.SendKeys(password + Keys.Enter);

                    webDriverWait.Until((p) => p.Url.StartsWith(ServerAddress.ToString(), StringComparison.OrdinalIgnoreCase));
                });
        }

        [SkippableFact]
        public void Can_Sign_In_With_Microsoft_Account()
        {
            string? userName = Environment.GetEnvironmentVariable("WEBSITE_USER_MICROSOFT_USERNAME");
            string? password = Environment.GetEnvironmentVariable("WEBSITE_USER_MICROSOFT_PASSWORD");

            Skip.If(string.IsNullOrWhiteSpace(userName), "No Microsoft Account username is configured.");
            Skip.If(string.IsNullOrWhiteSpace(password), "No Microsoft Account password is configured.");

            SignInWithSocialProvider(
                "microsoft",
                (driver) =>
                {
                    var userNameSelector = By.CssSelector("input[type=email]");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    var userNameElement = driver.FindElement(userNameSelector);
                    userNameElement.SendKeys(userName + Keys.Enter);

                    var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    webDriverWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                    webDriverWait.Until((p) => p.FindElement(passwordSelector).Displayed);

                    var passwordElement = driver.FindElement(passwordSelector);
                    passwordElement.SendKeys(password + Keys.Enter);

                    webDriverWait.Until((p) => p.Url.StartsWith(ServerAddress.ToString(), StringComparison.OrdinalIgnoreCase));
                });
        }

        [SkippableFact]
        public void Can_Sign_In_With_Twitter()
        {
            string? userName = Environment.GetEnvironmentVariable("WEBSITE_USER_TWITTER_USERNAME");
            string? password = Environment.GetEnvironmentVariable("WEBSITE_USER_TWITTER_PASSWORD");

            Skip.If(string.IsNullOrWhiteSpace(userName), "No Twitter username is configured.");
            Skip.If(string.IsNullOrWhiteSpace(password), "No Twitter password is configured.");

            SignInWithSocialProvider(
                "twitter",
                (driver) =>
                {
                    var userNameSelector = By.Id("username_or_email");
                    var passwordSelector = By.CssSelector("input[type=password]");

                    var userNameElement = driver.FindElement(userNameSelector);
                    userNameElement.SendKeys(userName);

                    var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    webDriverWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                    webDriverWait.Until((p) => p.FindElement(passwordSelector).Displayed);

                    var passwordElement = driver.FindElement(passwordSelector);
                    passwordElement.SendKeys(password + Keys.Enter);

                    webDriverWait.Until((p) => p.Url.StartsWith(ServerAddress.ToString(), StringComparison.OrdinalIgnoreCase));
                });
        }

        private void SignInWithSocialProvider(string providerName, Action<IWebDriver> signIn)
        {
            // Arrange
            AtPage<HomePage>(
                (page) =>
                {
                    page = page.SignIn()
                               .SignInWithProvider(providerName);

                    signIn(page.Navigator.Driver);

                    // Assert
                    page.IsAuthenticated().ShouldBeTrue();
                    page.UserName().ShouldNotBeNullOrWhiteSpace();

                    // Act
                    page = page.SignOut();

                    // Assert
                    page.IsAuthenticated().ShouldBeFalse();
                });
        }
    }
}
