// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class ExternalSignInPageTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OnPost_Returns_Bad_Request_If_No_Provider(string? provider)
    {
        // Arrange
        var page = CreateTarget();

        // Act
        var actual = page.OnPost(provider);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public void OnPost_Returns_Not_Found_If_Sign_In_Is_Disabled()
    {
        // Arrange
        var page = CreateTarget(PageModelHelpers.CreateSiteOptions(isEnabled: false));

        // Act
        var actual = page.OnPost("Amazon");

        // Assert
        actual.ShouldBeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(null, null, "/account/external-sign-in-callback/", "/account/sign-in/")]
    [InlineData("/manage/", null, "/account/external-sign-in-callback/?ReturnUrl=%2Fmanage%2F", "/account/sign-in/")]
    [InlineData(null, "https://localhost/account/sign-in/", "/account/external-sign-in-callback/", "/account/sign-in/")]
    [InlineData(null, "https://localhost/account/register/", "/account/external-sign-in-callback/", "/account/register/")]
    [InlineData("/manage/", "https://localhost/account/register/?Message=LinkFailed", "/account/external-sign-in-callback/?ReturnUrl=%2Fmanage%2F", "/account/register/")]
    public void OnPost_Challenges_Provider(string? returnUrl, string? referrer, string expectedRedirect, string expectedErrorRedirect)
    {
        // Arrange
        var page = CreateTarget();

        if (referrer is not null)
        {
            page.Request.Headers.Referer = referrer;
        }

        // Act
        var actual = page.OnPost("Amazon", returnUrl);

        // Assert
        var challenge = actual.ShouldBeOfType<ChallengeResult>();
        challenge.AuthenticationSchemes.ShouldBe(["Amazon"]);
        challenge.Properties.ShouldNotBeNull();
        challenge.Properties.RedirectUri.ShouldBe(expectedRedirect);
        challenge.Properties.Items.ShouldContainKeyAndValue(SiteContext.ErrorRedirectPropertyName, expectedErrorRedirect);
    }

    private ExternalSignIn CreateTarget(SiteOptions? options = null)
    {
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        signInManager.ConfigureExternalAuthenticationProperties("Amazon", Arg.Any<string?>(), Arg.Any<string?>())
                     .Returns((call) => new() { RedirectUri = call.ArgAt<string?>(1) });

        var httpContext = PageModelHelpers.CreateHttpContext(
            outputHelper,
            PageModelHelpers.CreatePrincipal(isAuthenticated: false));

        var page = new ExternalSignIn(signInManager, options ?? PageModelHelpers.CreateSiteOptions());

        return page.WithPageContext(httpContext);
    }
}
