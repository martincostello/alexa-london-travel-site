// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public partial class ExternalSignInCallback(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    TimeProvider timeProvider,
    SiteOptions siteOptions,
    ILogger<ExternalSignInCallback> logger) : PageModel
{
    /// <summary>
    /// Gets a value indicating whether the account of the user is locked out.
    /// </summary>
    public bool IsLockedOut { get; private set; }

    public async Task<IActionResult> OnGet(string? returnUrl = null, string? remoteError = null)
    {
        if (!AccountHelpers.IsSignInEnabled(siteOptions))
        {
            return NotFound();
        }

        if (remoteError != null)
        {
            Log.RemoteSignInError(logger, remoteError);
            return Page();
        }

        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            return RedirectToPage(SiteRoutes.SignIn);
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
            IsLockedOut = true;
            return Page();
        }

        var user = CreateSystemUser(info);

        if (!Uri.TryCreate(returnUrl, UriKind.Relative, out var returnUri))
        {
            returnUri = null;
        }

        if (string.IsNullOrEmpty(user?.Email))
        {
            ViewData["Message"] = nameof(SiteMessage.PermissionDenied);
            ViewData["ReturnUrl"] = returnUri?.ToString();

            return Page();
        }

        var identityResult = await userManager.CreateAsync(user);

        if (identityResult.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: true);

            Log.UserCreated(logger, user.Id, info.LoginProvider);

            return returnUri != null && AccountHelpers.IsAlexaAuthorization(returnUri.ToString())
                ? Redirect(returnUri.ToString())
                : RedirectToPage(SiteRoutes.Home, new { Message = SiteMessage.AccountCreated });
        }

        bool isUserAlreadyRegistered = identityResult.Errors.Any((p) => p.Code.StartsWith("Duplicate", StringComparison.Ordinal));

        AddErrors(identityResult);

        if (isUserAlreadyRegistered)
        {
            ViewData["Message"] = nameof(SiteMessage.AlreadyRegistered);
            ViewData["ReturnUrl"] = Url.Page(SiteRoutes.Manage);
        }
        else
        {
            ViewData["ReturnUrl"] = returnUrl;
        }

        return Page();
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
        return returnUrl != null && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToPage(SiteRoutes.Home);
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

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
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
