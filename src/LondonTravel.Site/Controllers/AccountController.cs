// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers;

[Authorize]
[Route("account", Name = SiteRoutes.Account)]
public partial class AccountController(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    TimeProvider timeProvider,
    SiteOptions siteOptions,
    ILogger<AccountController> logger) : Controller
{
    private readonly bool _isEnabled =
        siteOptions?.Authentication?.IsEnabled == true &&
        siteOptions?.Authentication.ExternalProviders?.Any((p) => p.Value?.IsEnabled == true) == true;

    [AllowAnonymous]
    [HttpGet]
    [Route("sign-in", Name = SiteRoutes.SignIn)]
    public async Task<IActionResult> SignIn(string? returnUrl = null)
    {
        if (!_isEnabled)
        {
            return NotFound();
        }

        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage(SiteRoutes.Home);
        }

        Uri? returnUri = null;

        if (returnUrl != null &&
            !Uri.TryCreate(returnUrl, UriKind.Relative, out returnUri))
        {
            return RedirectToPage(SiteRoutes.Home);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;

        string viewName = nameof(SignIn);

        if (IsRedirectAlexaAuthorization(returnUri?.ToString()))
        {
            viewName += "Alexa";
            string[] authenticationSchemesDisabledForAlexa = ["apple", "github", "google"];
            ViewData["AuthenticationSchemesToHide"] = authenticationSchemesDisabledForAlexa;
        }

        return View(viewName);
    }

    [HttpPost]
    [Route("sign-out", Name = SiteRoutes.SignOut)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOutPost()
    {
        if (!_isEnabled)
        {
            return NotFound();
        }

        string? userId = userManager.GetUserId(User);

        await signInManager.SignOutAsync();

        Log.UserSignedOut(logger, userId);

        return RedirectToPage(SiteRoutes.Home);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("external-sign-in", Name = SiteRoutes.ExternalSignIn)]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalSignIn(string? provider, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest();
        }

        if (!_isEnabled)
        {
            return NotFound();
        }

        string? redirectUrl = Url.RouteUrl(SiteRoutes.ExternalSignInCallback, new { ReturnUrl = returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        string errorRedirectUrl = GetErrorRedirectUrl();
        SiteContext.SetErrorRedirect(properties, errorRedirectUrl);

        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    [Route("external-sign-in-callback", Name = SiteRoutes.ExternalSignInCallback)]
    [HttpGet]
    public async Task<IActionResult> ExternalSignInCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!_isEnabled)
        {
            return NotFound();
        }

        if (remoteError != null)
        {
            Log.RemoteSignInError(logger, remoteError);
            return View(nameof(SignIn));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            return RedirectToRoute(SiteRoutes.SignIn);
        }

        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);

        if (result.Succeeded)
        {
            string? userId = userManager.GetUserId(info.Principal);

            Log.UserSignedIn(logger, userId, info.LoginProvider);

            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            return View("LockedOut");
        }
        else
        {
            var user = CreateSystemUser(info);

            if (!Uri.TryCreate(returnUrl, UriKind.Relative, out var returnUri))
            {
                returnUri = null;
            }

            if (string.IsNullOrEmpty(user?.Email))
            {
                ViewData["Message"] = nameof(SiteMessage.PermissionDenied);
                ViewData["ReturnUrl"] = returnUri?.ToString();

                return View("SignIn");
            }

            var identityResult = await userManager.CreateAsync(user);

            if (identityResult.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: true);

                Log.UserCreated(logger, user.Id, info.LoginProvider);

                return returnUri != null && IsRedirectAlexaAuthorization(returnUri.ToString()) ?
                    Redirect(returnUri.ToString()) :
                    RedirectToPage(SiteRoutes.Home, new { Message = SiteMessage.AccountCreated });
            }

            bool isUserAlreadyRegistered = identityResult.Errors.Any((p) => p.Code.StartsWith("Duplicate", StringComparison.Ordinal));

            AddErrors(identityResult);

            if (isUserAlreadyRegistered)
            {
                ViewData["Message"] = nameof(SiteMessage.AlreadyRegistered);
                ViewData["ReturnUrl"] = Url.RouteUrl("Manage");
            }
            else
            {
                ViewData["ReturnUrl"] = returnUrl;
            }

            return View("SignIn");
        }
    }

    private static bool IsRedirectAlexaAuthorization(string? returnUrl) => IsUrlOtherUrl(returnUrl, "/alexa/authorize");

    private static bool IsUrlOtherUrl(string? url, string targetUrl)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        if (uri.IsAbsoluteUri)
        {
            return string.Equals(uri.AbsolutePath, targetUrl, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            int indexOfQuery = url.IndexOf('?', StringComparison.Ordinal);

            if (indexOfQuery > -1)
            {
                url = url[..indexOfQuery];
            }

            return string.Equals(url.TrimEnd('/'), targetUrl!.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            Log.IdentityError(logger, error.Code, error.Description);
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return returnUrl != null && Url.IsLocalUrl(returnUrl) ?
            Redirect(returnUrl) :
            RedirectToPage(SiteRoutes.Home);
    }

    private LondonTravelUser? CreateSystemUser(ExternalLoginInfo info)
    {
        string? email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            return null;
        }

        string? givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        string? surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

        var user = new LondonTravelUser()
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            Email = email,
            GivenName = givenName,
            Surname = surname,
            UserName = email,
            EmailConfirmed = false,
        };

        user.Logins.Add(LondonTravelLoginInfo.FromUserLoginInfo(info));

        foreach (var claim in info.Principal.Claims)
        {
            user.RoleClaims.Add(LondonTravelRole.FromClaim(claim));
        }

        return user;
    }

    private string GetErrorRedirectUrl()
    {
        return (IsReferrerRegistrationPage() ? Url.Page(SiteRoutes.Register) : Url.RouteUrl(SiteRoutes.SignIn))!;
    }

    private bool IsReferrerRegistrationPage() => IsReferrerPageOrRoute(SiteRoutes.Register);

    private bool IsReferrerPageOrRoute(string routeName)
    {
        return IsUrlPageOrRoute(HttpContext.Request.Headers.Referer, routeName);
    }

    private bool IsUrlPageOrRoute(string? url, string pageOrRouteName)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        string? targetUrl = Url.RouteUrl(pageOrRouteName);

        targetUrl ??= Url.Page(pageOrRouteName);

        if (uri.IsAbsoluteUri)
        {
            return string.Equals(uri.AbsolutePath, targetUrl, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            int indexOfQuery = url.IndexOf('?', StringComparison.Ordinal);

            if (indexOfQuery > -1)
            {
                url = url[..indexOfQuery];
            }

            return string.Equals(url.TrimEnd('/'), targetUrl!.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "User Id {UserId} signed out.")]
        public static partial void UserSignedOut(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Warning,
            Message = "Error from external provider. {RemoteError}")]
        public static partial void RemoteSignInError(ILogger logger, string remoteError);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Information,
            Message = "User Id {UserId} signed in with provider {LoginProvider}.")]
        public static partial void UserSignedIn(ILogger logger, string? userId, string loginProvider);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Information,
            Message = "New user account {UserId} created through {LoginProvider}.")]
        public static partial void UserCreated(ILogger logger, string? userId, string loginProvider);

        [LoggerMessage(
           EventId = 5,
           Level = LogLevel.Warning,
           Message = "{ErrorCode}: {ErrorDescription}")]
        public static partial void IdentityError(ILogger logger, string errorCode, string errorDescription);
    }
}
