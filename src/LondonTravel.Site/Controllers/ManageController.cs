// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers;

[Authorize]
[Route("manage", Name = SiteRoutes.Manage)]
public partial class ManageController : Controller
{
    private readonly UserManager<LondonTravelUser> _userManager;
    private readonly SignInManager<LondonTravelUser> _signInManager;
    private readonly ITflServiceFactory _tflServiceFactory;
    private readonly ISiteTelemetry _telemetry;
    private readonly ILogger _logger;

    public ManageController(
      UserManager<LondonTravelUser> userManager,
      SignInManager<LondonTravelUser> signInManager,
      ITflServiceFactory tflServiceFactory,
      ISiteTelemetry telemetry,
      ILogger<ManageController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tflServiceFactory = tflServiceFactory;
        _telemetry = telemetry;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            Log.FailedToGetUserToManageAccount(_logger);
            return View("Error");
        }

        var userLogins = await _userManager.GetLoginsAsync(user);

        var otherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where((p) => userLogins.All((r) => p.Name != r.LoginProvider))
            .OrderBy((p) => p.DisplayName)
            .ThenBy((p) => p.Name)
            .ToList();

        foreach (var login in userLogins)
        {
            if (string.IsNullOrWhiteSpace(login.ProviderDisplayName))
            {
                login.ProviderDisplayName = login.LoginProvider;
            }
        }

        userLogins = userLogins
            .OrderBy((p) => p.ProviderDisplayName)
            .ThenBy((p) => p.LoginProvider)
            .ToList();

        var model = new ManageViewModel()
        {
            CurrentLogins = userLogins,
            ETag = user.ETag!,
            IsLinkedToAlexa = !string.IsNullOrWhiteSpace(user.AlexaToken),
            OtherLogins = otherLogins,
        };

        return View(model);
    }

    [HttpPost]
    [Route("link-account", Name = SiteRoutes.LinkAccount)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkAccount(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest();
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        string? redirectUrl = Url.RouteUrl(SiteRoutes.LinkAccountCallback);
        string? userId = _userManager.GetUserId(User);

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);

        SiteContext.SetErrorRedirect(properties, Url.RouteUrl(SiteRoutes.Manage)!);

        Log.AttemptingToLinkUser(_logger, userId, provider);

        _telemetry.TrackLinkExternalAccountStart(userId, provider);

        return Challenge(properties, provider);
    }

    [HttpGet]
    [Route("link-account-callback", Name = SiteRoutes.LinkAccountCallback)]
    public async Task<ActionResult> LinkAccountCallback()
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            Log.FailedToGetUserToManageAccount(_logger);
            return View("Error");
        }

        string? userId = await _userManager.GetUserIdAsync(user);
        var info = await _signInManager.GetExternalLoginInfoAsync(userId);

        if (info == null)
        {
            Log.FailedToGetExternalLogin(_logger, userId);
            return RedirectToRoute(SiteRoutes.Manage, new { Message = SiteMessage.Error });
        }

        Log.AddingExternalLogin(_logger, info.LoginProvider, userId);

        var result = await _userManager.AddLoginAsync(user, info);
        var message = SiteMessage.Error;

        if (result.Succeeded)
        {
            _telemetry.TrackLinkExternalAccountSuccess(userId, info.LoginProvider);

            Log.AddedExternalLogin(_logger, info.LoginProvider, userId);

            message = SiteMessage.LinkSuccess;

            result = await UpdateClaimsAsync(user, info);

            if (result.Succeeded)
            {
                Log.UpdatedUserClaims(_logger, userId, info.LoginProvider);
            }
            else
            {
                Log.UpdatingUserClaimsFailed(
                    _logger,
                    userId,
                    info.LoginProvider,
                    FormatErrors(result));
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
        else
        {
            _telemetry.TrackLinkExternalAccountFailed(userId, info.LoginProvider);

            Log.AddingExternalLoginFailed(
                _logger,
                userId,
                FormatErrors(result));
        }

        return RedirectToRoute(SiteRoutes.Manage, new { Message = message });
    }

    [HttpPost]
    [Route("remove-account-link", Name = SiteRoutes.RemoveAccountLink)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAccountLink(RemoveExternalService account)
    {
        if (account == null ||
            string.IsNullOrWhiteSpace(account.LoginProvider) ||
            string.IsNullOrWhiteSpace(account.ProviderKey))
        {
            return BadRequest();
        }

        var user = await GetCurrentUserAsync();
        var message = SiteMessage.Error;

        if (user != null)
        {
            Log.RemovingExternalLogin(_logger, account.LoginProvider, user.Id);

            var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

            if (result.Succeeded)
            {
                Log.RemovedExternalLogin(_logger, account.LoginProvider, user.Id);

                await _signInManager.SignInAsync(user, isPersistent: true);

                _telemetry.TrackRemoveExternalAccountLink(user.Id!, account.LoginProvider);

                message = SiteMessage.RemoveAccountLinkSuccess;
            }
            else
            {
                Log.RemovingExternalLoginFailed(
                    _logger,
                    user.Id,
                    account.LoginProvider,
                    FormatErrors(result));
            }
        }

        return RedirectToRoute(SiteRoutes.Manage, new { Message = message });
    }

    [HttpPost]
    [Route("remove-alexa-link", Name = SiteRoutes.RemoveAlexaLink)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAlexaLink(string etag)
    {
        if (string.IsNullOrWhiteSpace(etag))
        {
            return BadRequest();
        }

        var user = await GetCurrentUserAsync();
        var message = SiteMessage.Error;

        if (user != null)
        {
            Log.RemovingAlexaLink(_logger, user.Id);

            user.AlexaToken = null;
            user.ETag = etag;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _telemetry.TrackRemoveAlexaLink(user.Id!);

                Log.RemovedAlexaLink(_logger, user.Id);

                message = SiteMessage.RemoveAlexaLinkSuccess;
            }
            else
            {
                Log.RemovingAlexaLinkFailed(_logger, user.Id, FormatErrors(result));
            }
        }

        return RedirectToRoute(SiteRoutes.Manage, new { Message = message });
    }

    [HttpPost]
    [Route("delete-account", Name = SiteRoutes.DeleteAccount)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await GetCurrentUserAsync();

        if (user is not null)
        {
            Log.DeletingUser(_logger, user.Id);

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                Log.DeletedUser(_logger, user.Id);

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                _telemetry.TrackAccountDeleted(user.Id!, user.Email!);

                return RedirectToPage(SiteRoutes.Home, new { Message = SiteMessage.AccountDeleted });
            }
            else
            {
                Log.DeletingUserFailed(_logger, user.Id, FormatErrors(result));
            }
        }
        else
        {
            Log.FailedToGetUserToDeleteAccount(_logger);
        }

        return RedirectToRoute(SiteRoutes.Manage, new { Message = SiteMessage.Error });
    }

    [HttpPost]
    [Route("update-line-preferences", Name = SiteRoutes.UpdateLinePreferences)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLinePreferences(
        [Bind(nameof(UpdateLinePreferencesViewModel.ETag), nameof(UpdateLinePreferencesViewModel.FavoriteLines))]
        UpdateLinePreferencesViewModel model,
        CancellationToken cancellationToken)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.ETag))
        {
            return BadRequest();
        }

        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            Log.FailedToGetUserToUpdateLinePreferences(_logger);
            return View("Error");
        }

        bool? updated = null;

        // Do not bother updating the preferences if they are they same
        bool hasModelBeenUpdated =
            model.FavoriteLines == null ||
            !model.FavoriteLines.SequenceEqual(user.FavoriteLines);

        if (hasModelBeenUpdated)
        {
            if (!await AreLinesValidAsync(model, cancellationToken))
            {
                return BadRequest();
            }

            Log.UpdatingLinePreferences(_logger, user.Id);

            var existingLines = user.FavoriteLines;
            var newLines = user.FavoriteLines = (model.FavoriteLines ?? Array.Empty<string>())
                .OrderBy((p) => p, StringComparer.Ordinal)
                .ToArray();

            // Override the ETag with the one in the model to ensure write consistency
            user.ETag = model.ETag;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _telemetry.TrackLinePreferencesUpdated(user.Id!, existingLines, newLines);
                Log.UpdatedLinePreferences(_logger, user.Id);
            }
            else
            {
                Log.UpdatingLinePreferencesFailed(_logger, user.Id, model.ETag);
            }

            updated = result.Succeeded;
        }

        return RedirectToPage(SiteRoutes.Home, new { UpdateSuccess = updated });
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join(';', result.Errors.Select((p) => $"{p.Code}: {p.Description}"));
    }

    private async Task<bool> AreLinesValidAsync(UpdateLinePreferencesViewModel model, CancellationToken cancellationToken)
    {
        if (model.FavoriteLines != null)
        {
            ITflService service = _tflServiceFactory.CreateService();
            ICollection<LineInfo> lines = await service.GetLinesAsync(cancellationToken);

            var validLines = lines.Select((p) => p.Id).ToList();

            return model.FavoriteLines.All((p) => validLines.Contains(p));
        }

        return true;
    }

    private async Task<LondonTravelUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(HttpContext.User);
    }

    private async Task<IdentityResult> UpdateClaimsAsync(LondonTravelUser user, ExternalLoginInfo info)
    {
        bool commitUpdate = false;

        if (user.RoleClaims == null)
        {
            user.RoleClaims = new List<LondonTravelRole>();
            commitUpdate = true;
        }

        foreach (var claim in info.Principal.Claims)
        {
            bool hasClaim = user?.RoleClaims
                .Where((p) => p.ClaimType == claim.Type)
                .Where((p) => p.Issuer == claim.Issuer)
                .Where((p) => p.Value == claim.Value)
                .Where((p) => p.ValueType == claim.ValueType)
                .Any() == true;

            if (!hasClaim)
            {
                user!.RoleClaims.Add(LondonTravelRole.FromClaim(claim));
                commitUpdate = true;
            }
        }

        if (commitUpdate)
        {
            var result = (await _userManager.UpdateAsync(user)) ?? IdentityResult.Failed();

            if (result.Succeeded)
            {
                _telemetry.TrackClaimsUpdated(user.Id!);
            }

            return result;
        }
        else
        {
            return IdentityResult.Success;
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
           EventId = 1,
           Level = LogLevel.Error,
           Message = "Failed to get user to manage account.")]
        public static partial void FailedToGetUserToManageAccount(ILogger logger);

        [LoggerMessage(
           EventId = 2,
           Level = LogLevel.Information,
           Message = "Attempting to link user Id {UserId} to provider {ProviderName}.")]
        public static partial void AttemptingToLinkUser(ILogger logger, string? userId, string providerName);

        [LoggerMessage(
           EventId = 3,
           Level = LogLevel.Error,
           Message = "Failed to get external login info for user Id {UserId} to link account.")]
        public static partial void FailedToGetExternalLogin(ILogger logger, string userId);

        [LoggerMessage(
           EventId = 4,
           Level = LogLevel.Trace,
           Message = "Adding login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddingExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
           EventId = 5,
           Level = LogLevel.Information,
           Message = "Added login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddedExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
           EventId = 6,
           Level = LogLevel.Information,
           Message = "Updated claims for user Id {UserId} for provider {ProviderName}.")]
        public static partial void UpdatedUserClaims(ILogger logger, string userId, string providerName);

        [LoggerMessage(
           EventId = 7,
           Level = LogLevel.Error,
           Message = "Failed to update user Id {UserId} with additional role claims for provider {ProviderName}: {Errors}.")]
        public static partial void UpdatingUserClaimsFailed(ILogger logger, string userId, string providerName, string errors);

        [LoggerMessage(
           EventId = 8,
           Level = LogLevel.Error,
           Message = "Failed to add external login info for user Id {UserId}: {Errors}.")]
        public static partial void AddingExternalLoginFailed(ILogger logger, string userId, string errors);

        [LoggerMessage(
           EventId = 9,
           Level = LogLevel.Trace,
           Message = "Removing login for provider {ProviderName} from user Id {UserId}.")]
        public static partial void RemovingExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
           EventId = 10,
           Level = LogLevel.Information,
           Message = "Removed login for {ProviderName} from user Id {UserId}.")]
        public static partial void RemovedExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
           EventId = 11,
           Level = LogLevel.Error,
           Message = "Failed to remove external login info from user Id {UserId} for provider {ProviderName}: {Errors}.")]
        public static partial void RemovingExternalLoginFailed(ILogger logger, string? userId, string providerName, string errors);

        [LoggerMessage(
           EventId = 12,
           Level = LogLevel.Trace,
           Message = "Removing Alexa link from user Id {UserId}.")]
        public static partial void RemovingAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 13,
           Level = LogLevel.Information,
           Message = "Removed Alexa link from user Id {UserId}.")]
        public static partial void RemovedAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 14,
           Level = LogLevel.Error,
           Message = "Failed to remove Alexa link from user Id {UserId}: {Errors}.")]
        public static partial void RemovingAlexaLinkFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 15,
           Level = LogLevel.Trace,
           Message = "Deleting user Id {UserId}.")]
        public static partial void DeletingUser(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 16,
           Level = LogLevel.Information,
           Message = "Deleted user Id {UserId}.")]
        public static partial void DeletedUser(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 17,
           Level = LogLevel.Error,
           Message = "Failed to delete user Id {UserId}: {Errors}.")]
        public static partial void DeletingUserFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 18,
           Level = LogLevel.Error,
           Message = "Failed to get user to delete account.")]
        public static partial void FailedToGetUserToDeleteAccount(ILogger logger);

        [LoggerMessage(
           EventId = 19,
           Level = LogLevel.Error,
           Message = "Failed to get user to update line preferences.")]
        public static partial void FailedToGetUserToUpdateLinePreferences(ILogger logger);

        [LoggerMessage(
           EventId = 20,
           Level = LogLevel.Trace,
           Message = "Updating line preferences for user Id {UserId}.")]
        public static partial void UpdatingLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 21,
           Level = LogLevel.Information,
           Message = "Updated line preferences for user Id {UserId}.")]
        public static partial void UpdatedLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 22,
           Level = LogLevel.Warning,
           Message = "Failed to update line preferences for user '{UserId}' as it would cause a write conflict. ETag: {ETag}.")]
        public static partial void UpdatingLinePreferencesFailed(ILogger logger, string? userId, string etag);
    }
}
