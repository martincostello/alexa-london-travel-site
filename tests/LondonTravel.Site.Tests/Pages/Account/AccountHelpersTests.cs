// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public static class AccountHelpersTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("/", false)]
    [InlineData("/manage/", false)]
    [InlineData("/alexa/authorize", true)]
    [InlineData("/alexa/authorize/", true)]
    [InlineData("/ALEXA/AUTHORIZE/", true)]
    [InlineData("/alexa/authorize/?client_id=alexa-london-travel&response_type=token", true)]
    [InlineData("/alexa/authorize-not", false)]
    [InlineData("https://londontravel.martincostello.com/alexa/authorize", true)]
    [InlineData("https://londontravel.martincostello.com/alexa/authorize?client_id=alexa-london-travel", true)]
    [InlineData("https://londontravel.martincostello.com/manage/", false)]
    public static void IsAlexaAuthorization_Returns_Correct_Value(string? url, bool expected)
    {
        // Act
        bool actual = AccountHelpers.IsAlexaAuthorization(url);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "/account/register/", false)]
    [InlineData("/account/register/", null, false)]
    [InlineData("/account/register/", "", false)]
    [InlineData("/account/register", "/account/register/", true)]
    [InlineData("/account/register/", "/account/register/", true)]
    [InlineData("/account/register/?Message=LinkFailed", "/account/register/", true)]
    [InlineData("/account/sign-in/", "/account/register/", false)]
    [InlineData("https://londontravel.martincostello.com/account/register/", "/account/register/", true)]
    [InlineData("https://londontravel.martincostello.com/account/register/?Message=LinkFailed", "/account/register/", true)]
    [InlineData("https://londontravel.martincostello.com/account/sign-in/", "/account/register/", false)]
    public static void IsPath_Returns_Correct_Value(string? url, string? path, bool expected)
    {
        // Act
        bool actual = AccountHelpers.IsPath(url, path);

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public static void IsSignInEnabled_Returns_False_If_No_Options()
    {
        // Act and Assert
        AccountHelpers.IsSignInEnabled(null).ShouldBeFalse();
        AccountHelpers.IsSignInEnabled(new SiteOptions()).ShouldBeFalse();
        AccountHelpers.IsSignInEnabled(new SiteOptions() { Authentication = new() }).ShouldBeFalse();
    }

    [Fact]
    public static void IsSignInEnabled_Returns_False_If_Authentication_Disabled()
    {
        // Arrange
        var options = new SiteOptions()
        {
            Authentication = new()
            {
                IsEnabled = false,
                ExternalProviders = new Dictionary<string, ExternalSignInOptions>()
                {
                    ["Amazon"] = new() { IsEnabled = true },
                },
            },
        };

        // Act and Assert
        AccountHelpers.IsSignInEnabled(options).ShouldBeFalse();
    }

    [Fact]
    public static void IsSignInEnabled_Returns_False_If_No_Providers_Enabled()
    {
        // Arrange
        var options = new SiteOptions()
        {
            Authentication = new()
            {
                IsEnabled = true,
                ExternalProviders = new Dictionary<string, ExternalSignInOptions>()
                {
                    ["Amazon"] = new() { IsEnabled = false },
                },
            },
        };

        // Act and Assert
        AccountHelpers.IsSignInEnabled(options).ShouldBeFalse();
    }

    [Fact]
    public static void IsSignInEnabled_Returns_True_If_Any_Provider_Enabled()
    {
        // Arrange
        var options = new SiteOptions()
        {
            Authentication = new()
            {
                IsEnabled = true,
                ExternalProviders = new Dictionary<string, ExternalSignInOptions>()
                {
                    ["Amazon"] = new() { IsEnabled = false },
                    ["Google"] = new() { IsEnabled = true },
                },
            },
        };

        // Act and Assert
        AccountHelpers.IsSignInEnabled(options).ShouldBeTrue();
    }
}
