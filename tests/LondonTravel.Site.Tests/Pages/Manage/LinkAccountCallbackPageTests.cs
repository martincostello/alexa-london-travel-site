// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class LinkAccountCallbackPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnGet_Redirects_To_Error_If_User_Not_Found()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Error);
    }

    [Fact]
    public async Task OnGet_Redirects_To_Manage_With_Error_If_No_External_Login()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);

        await userManager.DidNotReceive().AddLoginAsync(Arg.Any<LondonTravelUser>(), Arg.Any<UserLoginInfo>());
    }

    [Fact]
    public async Task OnGet_Redirects_To_Manage_With_Error_If_Login_Cannot_Be_Added()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var info = ConfigureExternalLogin(signInManager);

        userManager.AddLoginAsync(user, info)
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "LoginAlreadyAssociated", Description = "Already linked." })));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnGet_Links_Account_And_Adds_New_Claims()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);
        var authenticationService = Substitute.For<IAuthenticationService>();

        var info = ConfigureExternalLogin(signInManager, new Claim(ClaimTypes.GivenName, "John"));

        userManager.AddLoginAsync(user, info)
                   .Returns(Task.FromResult(IdentityResult.Success));

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager, authenticationService);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.LinkSuccess);

        user.RoleClaims.ShouldHaveSingleItem();
        user.RoleClaims[0].ClaimType.ShouldBe(ClaimTypes.GivenName);
        user.RoleClaims[0].Value.ShouldBe("John");

        await userManager.Received(1).UpdateAsync(user);
        await authenticationService.Received(1).SignOutAsync(page.HttpContext, IdentityConstants.ExternalScheme, Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task OnGet_Links_Account_If_Claims_Cannot_Be_Updated()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var info = ConfigureExternalLogin(signInManager, new Claim(ClaimTypes.GivenName, "John"));

        userManager.AddLoginAsync(user, info)
                   .Returns(Task.FromResult(IdentityResult.Success));

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "ConcurrencyFailure", Description = "Write conflict." })));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.LinkSuccess);
    }

    [Fact]
    public async Task OnGet_Links_Account_And_Does_Not_Update_Existing_Claims()
    {
        // Arrange
        var claim = new Claim(ClaimTypes.GivenName, "John");

        var user = PageModelHelpers.CreateUser();
        user.RoleClaims = [LondonTravelRole.FromClaim(claim)];

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var info = ConfigureExternalLogin(signInManager, claim);

        userManager.AddLoginAsync(user, info)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.LinkSuccess);

        user.RoleClaims.ShouldHaveSingleItem();

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnGet_Links_Account_And_Initializes_Claims_If_Null()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        user.RoleClaims = null!;

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var info = ConfigureExternalLogin(signInManager);

        userManager.AddLoginAsync(user, info)
                   .Returns(Task.FromResult(IdentityResult.Success));

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        AssertRedirectToManage(actual, SiteMessage.LinkSuccess);

        user.RoleClaims.ShouldNotBeNull();
        user.RoleClaims.ShouldBeEmpty();

        await userManager.Received(1).UpdateAsync(user);
    }

    private static void AssertRedirectToManage(IActionResult actual, SiteMessage expected)
    {
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Manage);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKeyAndValue("Message", expected);
    }

    private static ExternalLoginInfo ConfigureExternalLogin(
        SignInManager<LondonTravelUser> signInManager,
        params Claim[] claims)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));
        var info = new ExternalLoginInfo(principal, "Google", "google-key", "Google");

        signInManager.GetExternalLoginInfoAsync(Arg.Any<string?>())
                     .Returns(Task.FromResult<ExternalLoginInfo?>(info));

        return info;
    }

    private LinkAccountCallback CreateTarget(
        UserManager<LondonTravelUser> userManager,
        SignInManager<LondonTravelUser> signInManager,
        IAuthenticationService? authenticationService = null)
    {
        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper, authenticationService: authenticationService);

        var page = new LinkAccountCallback(userManager, signInManager, outputHelper.ToLogger<LinkAccountCallback>());

        return page.WithPageContext(httpContext);
    }
}
