// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class IndexPageTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task OnGet_Redirects_To_Error_If_User_Not_Found()
    {
        // Arrange
        var page = CreateTarget(user: null);

        // Act
        var actual = await page.OnGet();

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Error);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("alexa-token", true)]
    public async Task OnGet_Renders_Linked_Accounts(string? alexaToken, bool expectedIsLinkedToAlexa)
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();
        user.AlexaToken = alexaToken;

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        IList<UserLoginInfo> logins =
        [
            new("Google", "google-key", null),
            new("Amazon", "amazon-key", "Amazon"),
        ];

        userManager.GetLoginsAsync(user)
                   .Returns(Task.FromResult(logins));

        IEnumerable<AuthenticationScheme> schemes =
        [
            new("Twitter", "Twitter", typeof(IAuthenticationHandler)),
            new("Google", "Google", typeof(IAuthenticationHandler)),
            new("Amazon", "Amazon", typeof(IAuthenticationHandler)),
            new("Apple", "Apple", typeof(IAuthenticationHandler)),
        ];

        signInManager.GetExternalAuthenticationSchemesAsync()
                     .Returns(Task.FromResult(schemes));

        var page = CreateTarget(user, userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        actual.ShouldBeOfType<PageResult>();

        page.ETag.ShouldBe("my-etag");
        page.IsLinkedToAlexa.ShouldBe(expectedIsLinkedToAlexa);

        page.CurrentLogins.Count.ShouldBe(2);
        page.CurrentLogins[0].LoginProvider.ShouldBe("Amazon");
        page.CurrentLogins[0].ProviderDisplayName.ShouldBe("Amazon");
        page.CurrentLogins[1].LoginProvider.ShouldBe("Google");
        page.CurrentLogins[1].ProviderDisplayName.ShouldBe("Google");
        page.ShowRemoveButton.ShouldBeTrue();

        page.OtherLogins.Count.ShouldBe(2);
        page.OtherLogins[0].Name.ShouldBe("Apple");
        page.OtherLogins[1].Name.ShouldBe("Twitter");
        page.CanAddMoreLogins.ShouldBeTrue();
    }

    [Fact]
    public async Task OnGet_Renders_Single_Linked_Account()
    {
        // Arrange
        var user = PageModelHelpers.CreateUser();

        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        IList<UserLoginInfo> logins = [new("Amazon", "amazon-key", "Amazon")];

        userManager.GetLoginsAsync(user)
                   .Returns(Task.FromResult(logins));

        IEnumerable<AuthenticationScheme> schemes = [new("Amazon", "Amazon", typeof(IAuthenticationHandler))];

        signInManager.GetExternalAuthenticationSchemesAsync()
                     .Returns(Task.FromResult(schemes));

        var page = CreateTarget(user, userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        actual.ShouldBeOfType<PageResult>();

        page.CurrentLogins.ShouldHaveSingleItem();
        page.ShowRemoveButton.ShouldBeFalse();
        page.OtherLogins.ShouldBeEmpty();
        page.CanAddMoreLogins.ShouldBeFalse();
    }

    private Index CreateTarget(
        LondonTravelUser? user,
        UserManager<LondonTravelUser>? userManager = null,
        SignInManager<LondonTravelUser>? signInManager = null)
    {
        userManager ??= PageModelHelpers.CreateUserManager(outputHelper, user);
        signInManager ??= PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper);

        var page = new Index(userManager, signInManager, outputHelper.ToLogger<Index>());

        return page.WithPageContext(httpContext);
    }
}
