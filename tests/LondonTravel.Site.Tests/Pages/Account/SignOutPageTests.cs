// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class SignOutPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnPost_Returns_Not_Found_If_Sign_In_Is_Disabled()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var page = CreateTarget(userManager, signInManager, PageModelHelpers.CreateSiteOptions(isEnabled: false));

        // Act
        var actual = await page.OnPost();

        // Assert
        actual.ShouldBeOfType<NotFoundResult>();

        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task OnPost_Signs_Out_And_Redirects_To_Home()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnPost();

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);

        await signInManager.Received(1).SignOutAsync();
    }

    private SignOut CreateTarget(
        UserManager<LondonTravelUser> userManager,
        SignInManager<LondonTravelUser> signInManager,
        SiteOptions? options = null)
    {
        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper);

        var page = new SignOut(
            userManager,
            signInManager,
            options ?? PageModelHelpers.CreateSiteOptions(),
            outputHelper.ToLogger<SignOut>());

        return page.WithPageContext(httpContext);
    }
}
