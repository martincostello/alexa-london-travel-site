// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class SignInPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnGet_Returns_Not_Found_If_Sign_In_Is_Disabled()
    {
        // Arrange
        var page = CreateTarget(options: PageModelHelpers.CreateSiteOptions(isEnabled: false));

        // Act
        var actual = await page.OnGet();

        // Assert
        actual.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnGet_Redirects_To_Home_If_Already_Signed_In()
    {
        // Arrange
        var page = CreateTarget(isAuthenticated: true);

        // Act
        var actual = await page.OnGet("/manage/");

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);
    }

    [Fact]
    public async Task OnGet_Redirects_To_Home_If_Return_Url_Is_Not_Relative()
    {
        // Arrange
        var page = CreateTarget();

        // Act
        var actual = await page.OnGet("https://www.example.com/");

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/manage/")]
    public async Task OnGet_Renders_Sign_In(string? returnUrl)
    {
        // Arrange
        var authenticationService = Substitute.For<IAuthenticationService>();
        var page = CreateTarget(authenticationService: authenticationService);

        // Act
        var actual = await page.OnGet(returnUrl);

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsAlexaAuthorization.ShouldBeFalse();
        page.ViewData["ReturnUrl"].ShouldBe(returnUrl);
        page.ViewData["AuthenticationSchemesToHide"].ShouldBeNull();

        await authenticationService.Received(1).SignOutAsync(page.HttpContext, IdentityConstants.ExternalScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task OnGet_Renders_Alexa_Sign_In()
    {
        // Arrange
        var page = CreateTarget();

        string returnUrl = "/alexa/authorize/?client_id=alexa-london-travel";

        // Act
        var actual = await page.OnGet(returnUrl);

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsAlexaAuthorization.ShouldBeTrue();
        page.ViewData["ReturnUrl"].ShouldBe(returnUrl);

        var hidden = page.ViewData["AuthenticationSchemesToHide"].ShouldBeAssignableTo<IEnumerable<string>>();
        hidden.ShouldBe(["apple", "github", "google"]);
    }

    private SignIn CreateTarget(
        bool isAuthenticated = false,
        SiteOptions? options = null,
        IAuthenticationService? authenticationService = null)
    {
        var httpContext = PageModelHelpers.CreateHttpContext(
            outputHelper,
            PageModelHelpers.CreatePrincipal(isAuthenticated),
            authenticationService);

        var page = new SignIn(options ?? PageModelHelpers.CreateSiteOptions());

        return page.WithPageContext(httpContext);
    }
}
