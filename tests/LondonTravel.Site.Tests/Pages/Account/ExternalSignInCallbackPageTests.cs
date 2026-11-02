// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class ExternalSignInCallbackPageTests(ITestOutputHelper outputHelper)
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
    public async Task OnGet_Renders_Sign_In_If_Remote_Error()
    {
        // Arrange
        var page = CreateTarget();

        // Act
        var actual = await page.OnGet(remoteError: "access_denied");

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsLockedOut.ShouldBeFalse();
        page.ViewData["Message"].ShouldBeNull();
    }

    [Fact]
    public async Task OnGet_Redirects_To_Sign_In_If_No_External_Login()
    {
        // Arrange
        var page = CreateTarget();

        // Act
        var actual = await page.OnGet();

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.SignIn);
    }

    [Theory]
    [InlineData("/manage/", "/manage/")]
    [InlineData("/alexa/authorize/?client_id=alexa-london-travel", "/alexa/authorize/?client_id=alexa-london-travel")]
    public async Task OnGet_Redirects_To_Return_Url_If_Sign_In_Succeeds(string returnUrl, string expected)
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Success);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet(returnUrl);

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://www.example.com/")]
    [InlineData("//www.example.com/")]
    public async Task OnGet_Redirects_To_Home_If_Sign_In_Succeeds_And_Return_Url_Not_Local(string? returnUrl)
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Success);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet(returnUrl);

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);
    }

    [Fact]
    public async Task OnGet_Renders_Locked_Out_If_Account_Is_Locked()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.LockedOut);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet();

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsLockedOut.ShouldBeTrue();
    }

    [Fact]
    public async Task OnGet_Renders_Permission_Denied_If_No_Email_Address()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Failed, email: null);

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet("/manage/");

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsLockedOut.ShouldBeFalse();
        page.ViewData["Message"].ShouldBe(nameof(SiteMessage.PermissionDenied));
        page.ViewData["ReturnUrl"].ShouldBe("/manage/");

        await userManager.DidNotReceive().CreateAsync(Arg.Any<LondonTravelUser>());
    }

    [Fact]
    public async Task OnGet_Creates_User_And_Redirects_To_Home()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Failed);

        LondonTravelUser? created = null;

        userManager.CreateAsync(Arg.Do<LondonTravelUser>((p) => created = p))
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet("/manage/");

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectToPageResult>();
        redirect.PageName.ShouldBe(SiteRoutes.Home);
        redirect.RouteValues.ShouldNotBeNull();
        redirect.RouteValues.ShouldContainKeyAndValue("Message", SiteMessage.AccountCreated);

        created.ShouldNotBeNull();
        created.Email.ShouldBe("john.smith@londontravel.martincostello.com");
        created.UserName.ShouldBe("john.smith@londontravel.martincostello.com");
        created.GivenName.ShouldBe("John");
        created.Surname.ShouldBe("Smith");
        created.EmailConfirmed.ShouldBeFalse();
        created.CreatedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
        created.Logins.ShouldHaveSingleItem();
        created.Logins[0].LoginProvider.ShouldBe("Amazon");
        created.Logins[0].ProviderKey.ShouldBe("amazon-key");
        created.RoleClaims.Count.ShouldBe(3);

        await signInManager.Received(1).SignInAsync(created, true, Arg.Any<string?>());
    }

    [Fact]
    public async Task OnGet_Creates_User_And_Redirects_To_Alexa_Authorization()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Failed);

        userManager.CreateAsync(Arg.Any<LondonTravelUser>())
                   .Returns(Task.FromResult(IdentityResult.Success));

        var page = CreateTarget(userManager, signInManager);

        string returnUrl = "/alexa/authorize/?client_id=alexa-london-travel&response_type=token";

        // Act
        var actual = await page.OnGet(returnUrl);

        // Assert
        var redirect = actual.ShouldBeOfType<RedirectResult>();
        redirect.Url.ShouldBe(returnUrl);
    }

    [Fact]
    public async Task OnGet_Renders_Already_Registered_If_User_Exists()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Failed);

        var error = new IdentityError() { Code = "DuplicateEmail", Description = "The email address is already in use." };

        userManager.CreateAsync(Arg.Any<LondonTravelUser>())
                   .Returns(Task.FromResult(IdentityResult.Failed(error)));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet("/help/");

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsLockedOut.ShouldBeFalse();
        page.ViewData["Message"].ShouldBe(nameof(SiteMessage.AlreadyRegistered));
        page.ViewData["ReturnUrl"].ShouldBe("/manage/");
        page.ModelState.ErrorCount.ShouldBe(1);

        await signInManager.DidNotReceive().SignInAsync(Arg.Any<LondonTravelUser>(), Arg.Any<bool>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task OnGet_Renders_Sign_In_If_User_Cannot_Be_Created()
    {
        // Arrange
        var userManager = PageModelHelpers.CreateUserManager(outputHelper);
        var signInManager = PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        ConfigureExternalLogin(signInManager, SignInResult.Failed);

        var error = new IdentityError() { Code = "Unknown", Description = "Something went wrong." };

        userManager.CreateAsync(Arg.Any<LondonTravelUser>())
                   .Returns(Task.FromResult(IdentityResult.Failed(error)));

        var page = CreateTarget(userManager, signInManager);

        // Act
        var actual = await page.OnGet("/help/");

        // Assert
        actual.ShouldBeOfType<PageResult>();
        page.IsLockedOut.ShouldBeFalse();
        page.ViewData["Message"].ShouldBeNull();
        page.ViewData["ReturnUrl"].ShouldBe("/help/");
        page.ModelState.ErrorCount.ShouldBe(1);
    }

    private static void ConfigureExternalLogin(
        SignInManager<LondonTravelUser> signInManager,
        SignInResult result,
        string? email = "john.smith@londontravel.martincostello.com")
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.GivenName, "John"),
            new(ClaimTypes.Surname, "Smith"),
        };

        if (email is not null)
        {
            claims.Add(new(ClaimTypes.Email, email));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Amazon"));
        var info = new ExternalLoginInfo(principal, "Amazon", "amazon-key", "Amazon");

        signInManager.GetExternalLoginInfoAsync(Arg.Any<string?>())
                     .Returns(Task.FromResult<ExternalLoginInfo?>(info));

        signInManager.ExternalLoginSignInAsync("Amazon", "amazon-key", true)
                     .Returns(Task.FromResult(result));
    }

    private ExternalSignInCallback CreateTarget(
        UserManager<LondonTravelUser>? userManager = null,
        SignInManager<LondonTravelUser>? signInManager = null,
        SiteOptions? options = null)
    {
        userManager ??= PageModelHelpers.CreateUserManager(outputHelper);
        signInManager ??= PageModelHelpers.CreateSignInManager(outputHelper, userManager);

        var httpContext = PageModelHelpers.CreateHttpContext(
            outputHelper,
            PageModelHelpers.CreatePrincipal(isAuthenticated: false));

        var page = new ExternalSignInCallback(
            userManager,
            signInManager,
            TimeProvider.System,
            options ?? PageModelHelpers.CreateSiteOptions(),
            outputHelper.ToLogger<ExternalSignInCallback>());

        return page.WithPageContext(httpContext);
    }
}
