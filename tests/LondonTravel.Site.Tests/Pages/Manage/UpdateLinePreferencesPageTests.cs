// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class UpdateLinePreferencesPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnPost_Returns_Bad_Request_If_Model_Null()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var page = CreateTarget(userManager);

        // Act
        var actual = await page.OnPost(null!, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task OnPost_Returns_Bad_Request_If_No_ETag(string etag)
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, PageModelHelpers.CreateUser());
        var page = CreateTarget(userManager);

        var model = new UpdateLinePreferencesViewModel() { ETag = etag, FavoriteLines = ["district"] };

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Redirects_To_Error_If_User_Not_Found()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var page = CreateTarget(userManager);

        var model = CreateModel("district");

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Error);
    }

    [Fact]
    public async Task OnPost_Returns_Bad_Request_If_Lines_Invalid()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var page = CreateTarget(userManager);

        var model = CreateModel("district", "not-a-line");

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Does_Not_Update_User_If_Lines_Unchanged()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var page = CreateTarget(userManager);

        var model = CreateModel([.. user.FavoriteLines]);

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        AssertRedirectToHome(actual, null);

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnPost_Updates_User_If_Lines_Changed()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager);

        var model = CreateModel("victoria", "central");

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        AssertRedirectToHome(actual, true);

        user.ETag.ShouldBe("new-etag");
        user.FavoriteLines.ShouldBe(["central", "victoria"]);
    }

    [Fact]
    public async Task OnPost_Clears_Lines_If_None_Specified()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager);

        var model = CreateModel();
        model.FavoriteLines = null!;

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        AssertRedirectToHome(actual, true);

        user.FavoriteLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task OnPost_Redirects_With_Failure_If_User_Cannot_Be_Updated()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);

        userManager.UpdateAsync(user)
                   .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() { Code = "ConcurrencyFailure", Description = "Write conflict." })));

        var page = CreateTarget(userManager);

        var model = CreateModel("victoria");

        // Act
        var actual = await page.OnPost(model, TestContext.Current.CancellationToken);

        // Assert
        AssertRedirectToHome(actual, false);
    }

    private static UpdateLinePreferencesViewModel CreateModel(params string[] favoriteLines)
        => new() { ETag = "new-etag", FavoriteLines = [.. favoriteLines] };

    private static void AssertRedirectToHome(IActionResult actual, bool? expected)
    {
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKey("UpdateSuccess");
        redirect.RouteValues["UpdateSuccess"].ShouldBe(expected);
    }

    private UpdateLinePreferences CreateTarget(UserManager<LondonTravelUser> userManager)
    {
        ICollection<LineInfo> lines =
        [
            new() { Id = "central", Name = "Central" },
            new() { Id = "district", Name = "District" },
            new() { Id = "northern", Name = "Northern" },
            new() { Id = "victoria", Name = "Victoria" },
        ];

        var service = Substitute.For<ITflService>();

        service.GetLinesAsync(Arg.Any<CancellationToken>())
               .Returns(Task.FromResult(lines));

        var factory = Substitute.For<ITflServiceFactory>();

        factory.CreateService()
               .Returns(service);

        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper);

        var page = new UpdateLinePreferences(userManager, factory, outputHelper.ToLogger<UpdateLinePreferences>());

        return page.WithPageContext(httpContext);
    }
}
