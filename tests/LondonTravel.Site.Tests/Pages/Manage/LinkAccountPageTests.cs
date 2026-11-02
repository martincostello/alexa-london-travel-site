// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

public class LinkAccountPageTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task OnPost_Returns_Bad_Request_If_No_Provider(string? provider)
    {
        // Arrange
        var page = CreateTarget(Substitute.For<IAuthenticationService>());

        // Act
        var actual = await page.OnPost(provider!);

        // Assert
        actual.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task OnPost_Challenges_Provider()
    {
        // Arrange
        var authenticationService = Substitute.For<IAuthenticationService>();
        var page = CreateTarget(authenticationService);

        // Act
        var actual = await page.OnPost("Google");

        // Assert
        var challenge = actual.ShouldBeOfType<ChallengeResult>();
        challenge.AuthenticationSchemes.ShouldBe(["Google"]);
        challenge.Properties.ShouldNotBeNull();
        challenge.Properties.RedirectUri.ShouldBe("/manage/link-account-callback/");
        challenge.Properties.Items.ShouldContainKeyAndValue(SiteContext.ErrorRedirectPropertyName, "/manage/");

        await authenticationService.Received(1).SignOutAsync(page.HttpContext, IdentityConstants.ExternalScheme, Arg.Any<AuthenticationProperties?>());
    }

    private LinkAccount CreateTarget(IAuthenticationService authenticationService)
    {
        var user = PageModelHelpers.CreateUser();
        var userManager = PageModelHelpers.CreateUserManager(outputHelper, user);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        signInManager.ConfigureExternalAuthenticationProperties("Google", Arg.Any<string?>(), user.Id)
                     .Returns((call) => new() { RedirectUri = call.ArgAt<string?>(1) });

        var httpContext = PageModelHelpers.CreateHttpContext(outputHelper, authenticationService: authenticationService);

        var page = new LinkAccount(userManager, signInManager, outputHelper.ToLogger<LinkAccount>());

        return page.WithPageContext(httpContext);
    }
}
