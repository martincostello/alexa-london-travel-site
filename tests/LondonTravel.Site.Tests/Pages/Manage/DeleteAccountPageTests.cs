// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class DeleteAccountPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_User_Not_Found()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost();

        // Assert
        AssertRedirect(actual, SiteRoutes.Manage, SiteMessage.Error);

        await userManager.DidNotReceive().DeleteAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_User_Cannot_Be_Deleted()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var authenticationService = Substitute.For<IAuthenticationService>();

        userManager.DeleteAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "ConcurrencyFailure", Description = "Write conflict." })));

        var page = CreateTarget(userManager, authenticationService);

        // Act
        var actual = await page.OnPost();

        // Assert
        AssertRedirect(actual, SiteRoutes.Manage, SiteMessage.Error);

        await authenticationService.DidNotReceive().SignOutAsync(Arg.Any<HttpContext>(), Arg.Any<string?>(), Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task OnPost_Deletes_User_And_Redirects_To_Home()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var authenticationService = Substitute.For<IAuthenticationService>();

        userManager.DeleteAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, authenticationService);

        // Act
        var actual = await page.OnPost();

        // Assert
        AssertRedirect(actual, SiteRoutes.Home, SiteMessage.AccountDeleted);

        await authenticationService.Received(1).SignOutAsync(page.HttpContext, IdentityConstants.ExternalScheme, Arg.Any<AuthenticationProperties?>());
        await authenticationService.Received(1).SignOutAsync(page.HttpContext, IdentityConstants.ApplicationScheme, Arg.Any<AuthenticationProperties?>());
    }

    private static void AssertRedirect(IActionResult actual, string expectedPage, SiteMessage expectedMessage)
    {
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(expectedPage);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKeyAndValue("Message", expectedMessage);
    }

    private DeleteAccount CreateTarget(
        UserManager<LondonTravelUser> userManager,
        IAuthenticationService? authenticationService = null)
    {
        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper, authenticationService: authenticationService);

        var page = new DeleteAccount(userManager, outputHelper.ToLogger<DeleteAccount>());

        return page.WithPageContext(httpContext);
    }
}
