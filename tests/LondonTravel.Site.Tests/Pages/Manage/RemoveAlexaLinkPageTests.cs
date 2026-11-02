// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class RemoveAlexaLinkPageTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task OnPost_Returns_Bad_Request_If_No_ETag(string? etag)
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost(etag!);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_User_Not_Found()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost("new-etag");

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Manage_With_Error_If_User_Cannot_Be_Updated()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        user.AlexaToken = "alexa-token";

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "ConcurrencyFailure", Description = "Write conflict." })));

        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost("new-etag");

        // Assert
        AssertRedirectToManage(actual, SiteMessage.Error);
    }

    [Fact]
    public async Task OnPost_Removes_Alexa_Link_And_Redirects_To_Manage()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        user.AlexaToken = "alexa-token";

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost("new-etag");

        // Assert
        AssertRedirectToManage(actual, SiteMessage.RemoveAlexaLinkSuccess);

        user.AlexaToken.ShouldBeNull();
        user.ETag.ShouldBe("new-etag");
    }

    private static void AssertRedirectToManage(IActionResult actual, SiteMessage expected)
    {
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Manage);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKeyAndValue("Message", expected);
    }

    private RemoveAlexaLink CreateTarget(UserManager<LondonTravelUser> userManager)
    {
        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper);

        var page = new RemoveAlexaLink(userManager, outputHelper.ToLogger<RemoveAlexaLink>());

        return page.WithPageContext(httpContext);
    }
}
