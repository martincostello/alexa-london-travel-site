// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class RemoveAccountLinkPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnPost_Returns_Bad_Request_If_Account_Null()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost(null!);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().RemoveLoginAsync(Arg.Any<LondonTravelUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("", "google-key")]
    [InlineData("  ", "google-key")]
    [InlineData("Google", "")]
    [InlineData("Google", "  ")]
    public async Task OnPost_Returns_Bad_Request_If_Account_Invalid(string loginProvider, string providerKey)
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var page = CreateTarget(userManager);

        var account = new RemoveExternalService() { LoginProvider = loginProvider, ProviderKey = providerKey };

        // Act
        var actual = await page.OnPost(account);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().RemoveLoginAsync(Arg.Any<LondonTravelUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_User_Not_Found()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost(CreateAccount());

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);

        await userManager.DidNotReceive().RemoveLoginAsync(Arg.Any<LondonTravelUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_Login_Cannot_Be_Removed()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        userManager.RemoveLoginAsync(user, "Google", "google-key")
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "ConcurrencyFailure", Description = "Write conflict." })));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnPost(CreateAccount());

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);

        await signInManager.DidNotReceive().SignInAsync(Arg.Any<LondonTravelUser>(), Arg.Any<bool>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task OnPost_Removes_Login_And_Redirects_To_Manage()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        userManager.RemoveLoginAsync(user, "Google", "google-key")
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnPost(CreateAccount());

        // Assert
        AssertRedirectToManage(actual, SiteMessage.RemoveAccountLinkSuccess);

        await signInManager.Received(1).SignInAsync(user, true, Arg.Any<string?>());
    }

    private static RemoveExternalService CreateAccount()
        => new() { LoginProvider = "Google", ProviderKey = "google-key" };

    private static void AssertRedirectToManage(IActionResult actual, SiteMessage expected)
    {
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Manage);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKeyAndValue("Message", expected);
    }

    private RemoveAccountLink CreateTarget(
        UserManager<LondonTravelUser> userManager,
        SignInManager<LondonTravelUser>? signInManager = null)
    {
        signInManager ??= PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper);

        var page = new RemoveAccountLink(userManager, signInManager, outputHelper.ToLogger<RemoveAccountLink>());

        return page.WithPageContext(httpContext);
    }
}
