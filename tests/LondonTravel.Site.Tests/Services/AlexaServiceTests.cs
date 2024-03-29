// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Services;

public class AlexaServiceTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Access_Tokens_Are_Randomly_Generated()
    {
        // Arrange
        var tokens = new List<string>();

        // Act
        for (int i = 0; i < 512; i++)
        {
            tokens.Add(AlexaService.GenerateAccessToken());
        }

        // Assert
        tokens.Distinct().Count().ShouldBe(tokens.Count);
        tokens.ShouldAllBe((p) => p.Length >= 64);
    }

    [Fact]
    public async Task AuthorizeSkill_Returns_Not_Found_If_Account_Linking_Disabled()
    {
        // Arrange
        string state = "Some State";
        string clientId = "SomeClientId";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        SiteOptions options = CreateValidSiteOptions();
        options.Alexa!.IsLinkingEnabled = false;

        var target = CreateTarget(options: options);

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        actual.ShouldNotBeNull();

        var httpContext = CreateHttpContext();

        await actual.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task AuthorizeSkill_Returns_Correct_Location_If_Client_Id_Is_Invalid(string? clientId)
    {
        // Arrange
        string state = "Some State";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=invalid_request");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("some-invalid-value")]
    public async Task AuthorizeSkill_Returns_Correct_Location_If_Client_Id_Is_Unauthorized(string clientId)
    {
        // Arrange
        string state = "Some State";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=unauthorized_client");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task AuthorizeSkill_Returns_Correct_Location_If_Response_Type_Is_Invalid(string? responseType)
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=invalid_request");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("code")]
    public async Task AuthorizeSkill_Returns_Correct_Location_If_Response_Type_Is_Not_Supported(string responseType)
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=unsupported_response_type");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/local")]
    [InlineData("http://alexa.amazon.com/alexa-london-travel")]
    [InlineData("http://alexa.amazon.com/alexa-london-travel/?foo=baz")]
    [InlineData("https://alexa.amazon.com/alexa-london-travel/unknown")]
    [InlineData("https://bbc.co.uk")]
    public async Task AuthorizeSkill_Returns_Bad_Request_If_Redirect_Uri_Is_Invalid(string? redirectUrl)
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        string responseType = "token";
        Uri? redirectUri = redirectUrl == null ? null : new Uri(redirectUrl, UriKind.RelativeOrAbsolute);

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        actual.ShouldNotBeNull();

        var httpContext = CreateHttpContext();

        await actual.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task AuthorizeSkill_Returns_Error_If_User_Not_Found()
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var target = CreateTarget();

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=server_error");
    }

    [Fact]
    public async Task AuthorizeSkill_Returns_Error_If_Update_Fails()
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var user = new LondonTravelUser();
        var result = IdentityResult.Failed(new IdentityError() { Code = "Error", Description = "Problem" });

        var userManager = CreateUserManager(user, result);

        var target = CreateTarget(userManager);

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        await AssertRedirect(
            actual,
            "https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&error=server_error");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("My Existing Token")]
    public async Task AuthorizeSkill_Returns_Correct_Redirect_Url_If_Token_Created_Or_Updated(string? alexaToken)
    {
        // Arrange
        string state = "Some State";
        string clientId = "my-client-id";
        string responseType = "token";
        var redirectUri = new Uri("https://alexa.amazon.com/alexa-london-travel?foo=bar");

        var user = new LondonTravelUser()
        {
            AlexaToken = alexaToken,
        };

        var result = IdentityResult.Success;

        var userManager = CreateUserManager(user, result);

        var target = CreateTarget(userManager);

        // Act
        IResult actual = await target.AuthorizeSkillAsync(
            state,
            clientId,
            responseType,
            redirectUri,
            new ClaimsPrincipal());

        // Assert
        actual.ShouldNotBeNull();

        var httpContext = CreateHttpContext();

        await actual.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status302Found);

        var location = httpContext.Response.Headers.Location;

        string url = location.ToString();

        url.ShouldNotBeNullOrWhiteSpace();
        url.ShouldStartWith("https://alexa.amazon.com/alexa-london-travel?foo=bar#state=Some%20State&access_token=");
        url.ShouldEndWith("&token_type=Bearer");

        if (alexaToken != null)
        {
            url.ShouldNotContain(alexaToken);
            url.ShouldNotContain(Uri.EscapeDataString(alexaToken));
        }

        user.AlexaToken.ShouldNotBeNull();
        user.AlexaToken.ShouldNotBe(alexaToken);
        user.AlexaToken.Length.ShouldBeGreaterThanOrEqualTo(64);
    }

    /// <summary>
    /// Creates an instance of <see cref="SiteOptions"/> that is valid for use with Alexa.
    /// </summary>
    /// <returns>
    /// The created instance of <see cref="SiteOptions"/>.
    /// </returns>
    private static SiteOptions CreateValidSiteOptions()
    {
        return new SiteOptions()
        {
            Alexa = new AlexaOptions()
            {
                ClientId = "my-client-id",
                IsLinkingEnabled = true,
                RedirectUrls =
                [
                    "https://alexa.amazon.com/alexa-london-travel?foo=bar",
                    "https://alexa.amazon.com/alexa-london-travel/link",
                ],
            },
        };
    }

    private UserManager<LondonTravelUser> CreateUserManager(LondonTravelUser? user = null, IdentityResult? result = null)
    {
        var manager = Substitute.For<UserManager<LondonTravelUser>>(
            Substitute.For<IUserStore<LondonTravelUser>>(),
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<IPasswordHasher<LondonTravelUser>>(),
            new[] { Substitute.For<IUserValidator<LondonTravelUser>>() },
            new[] { Substitute.For<IPasswordValidator<LondonTravelUser>>() },
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            outputHelper.ToLogger<UserManager<LondonTravelUser>>());

        if (user != null)
        {
            manager.GetUserAsync(Arg.Is<ClaimsPrincipal>((p) => p != null))
                   .Returns(Task.FromResult<LondonTravelUser?>(user));

            if (result != null)
            {
                manager.UpdateAsync(user)
                       .Returns(Task.FromResult(result));
            }
        }

        return manager;
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging((p) => p.AddXUnit(outputHelper))
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };
    }

    private AlexaService CreateTarget(
        UserManager<LondonTravelUser>? userManager = null,
        SiteOptions? options = null)
    {
        return new AlexaService(
            userManager ?? CreateUserManager(),
            Substitute.For<ISiteTelemetry>(),
            Microsoft.Extensions.Options.Options.Create(options ?? CreateValidSiteOptions()),
            outputHelper.ToLogger<AlexaService>());
    }

    private async Task<HttpContext> AssertRedirect(IResult actual, string? url = null)
    {
        actual.ShouldNotBeNull();

        var httpContext = CreateHttpContext();

        await actual.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status302Found);

        if (url != null)
        {
            var location = httpContext.Response.Headers.Location;
            location.ToString().ShouldBe(url);
        }

        return httpContext;
    }
}
