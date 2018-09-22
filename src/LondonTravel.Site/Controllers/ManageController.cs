// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services.Data;
    using Services.Tfl;
    using Telemetry;

    /// <summary>
    /// A class representing the controller for the <c>/manage/</c> resource.
    /// </summary>
    [Authorize]
    [Route("manage", Name = SiteRoutes.Manage)]
    public class ManageController : Controller
    {
        private readonly UserManager<LondonTravelUser> _userManager;
        private readonly SignInManager<LondonTravelUser> _signInManager;
        private readonly IDocumentService _documentService;
        private readonly ITflServiceFactory _tflServiceFactory;
        private readonly ISiteTelemetry _telemetry;
        private readonly ILogger _logger;

        public ManageController(
          UserManager<LondonTravelUser> userManager,
          SignInManager<LondonTravelUser> signInManager,
          IDocumentService documentService,
          ITflServiceFactory tflServiceFactory,
          ISiteTelemetry telemetry,
          ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _documentService = documentService;
            _tflServiceFactory = tflServiceFactory;
            _telemetry = telemetry;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/manage/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/manage/</c> action.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                _logger.LogError("Failed to get user to manage account.");

                return View("Error");
            }

            var userLogins = (await _userManager.GetLoginsAsync(user))
                .OrderBy((p) => p.ProviderDisplayName)
                .ThenBy((p) => p.LoginProvider)
                .ToList();

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

            var model = new ManageViewModel()
            {
                CurrentLogins = userLogins,
                ETag = user.ETag,
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

            var redirectUrl = Url.RouteUrl(SiteRoutes.LinkAccountCallback);
            var userId = _userManager.GetUserId(User);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);

            SiteContext.SetErrorRedirect(properties, Url.RouteUrl(SiteRoutes.Manage));

            _logger.LogInformation("Attempting to link user Id {UserId} to provider {ProviderName}.", userId, provider);

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
                _logger.LogError("Failed to get user to link account.");

                return View("Error");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);

            if (info == null)
            {
                _logger.LogError("Failed to get external login info for user Id {UserId} to link account.", userId);

                return RedirectToRoute(SiteRoutes.Manage, new { Message = SiteMessage.Error });
            }

            _logger.LogTrace("Adding login for provider {ProviderName} to user Id {UserId}.", info.LoginProvider, userId);

            var result = await _userManager.AddLoginAsync(user, info);
            var message = SiteMessage.Error;

            if (result.Succeeded)
            {
                _telemetry.TrackLinkExternalAccountSuccess(userId, info.LoginProvider);

                _logger.LogInformation("Added login for provider {ProviderName} to user Id {UserId}.", info.LoginProvider, userId);

                message = SiteMessage.LinkSuccess;

                result = await UpdateClaimsAsync(user, info);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Updated claims for user Id {UserId} for provider {ProviderName}.", userId, info.LoginProvider);
                }
                else
                {
                    _logger.LogError(
                        "Failed to update user Id {UserId} with additional role claims for provider {ProviderName}: {Errors}.",
                        userId,
                        info.LoginProvider,
                        FormatErrors(result));
                }

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
            else
            {
                _telemetry.TrackLinkExternalAccountFailed(userId, info.LoginProvider);

                _logger.LogError(
                    "Failed to add external login info for user Id {UserId}: {Errors}.",
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

            if (user != null && account != null)
            {
                _logger.LogTrace("Removing login for provider {ProviderName} from user Id {UserId}.", account.LoginProvider, user.Id);

                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "Removed login for {ProviderName} from user Id {UserId}.",
                        account.LoginProvider,
                        user.Id);

                    await _signInManager.SignInAsync(user, isPersistent: true);

                    _telemetry.TrackRemoveExternalAccountLink(user.Id, account.LoginProvider);

                    message = SiteMessage.RemoveAccountLinkSuccess;
                }
                else
                {
                    _logger.LogError(
                        "Failed to remove external login info from user Id {UserId} for provider {ProviderName}: {Errors}.",
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
                _logger.LogTrace("Removing Alexa link from user Id {UserId}.", user.Id);

                user.AlexaToken = null;
                user.ETag = etag;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _telemetry.TrackRemoveAlexaLink(user.Id);

                    _logger.LogInformation("Removed Alexa link from user Id {UserId}.", user.Id);

                    message = SiteMessage.RemoveAlexaLinkSuccess;
                }
                else
                {
                    _logger.LogError(
                        "Failed to remove Alexa link from user Id {UserId}: {Errors}.",
                        user.Id,
                        FormatErrors(result));
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

            if (user != null)
            {
                _logger.LogTrace("Deleting user Id {UserId}.", user.Id);

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Deleted user Id {UserId}.", user.Id);

                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                    _telemetry.TrackAccountDeleted(user.Id, user.Email);

                    return RedirectToRoute(SiteRoutes.Home, new { Message = SiteMessage.AccountDeleted });
                }
                else
                {
                    _logger.LogError(
                        "Failed to delete user Id {UserId}: {Errors}.",
                        user.Id,
                        FormatErrors(result));
                }
            }
            else
            {
                _logger.LogError("Failed to get user to delete account.");
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
                _logger.LogError("Failed to get user to update line preferences.");

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

                _logger.LogTrace("Updating line preferences for user Id {UserId}.", user.Id);

                var existingLines = user.FavoriteLines;
                var newLines = user.FavoriteLines = (model.FavoriteLines ?? Array.Empty<string>())
                    .OrderBy((p) => p, StringComparer.Ordinal)
                    .ToArray();

                // Override the ETag with the one in the model to ensure write consistency
                user.ETag = model.ETag;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _telemetry.TrackLinePreferencesUpdated(user.Id, existingLines, newLines);

                    _logger.LogInformation("Updated line preferences for user Id {UserId}.", user.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to update line preferences for user '{UserId}' as it would cause a write conflict. ETag: {ETag}.",
                        user.Id,
                        model.ETag);
                }

                updated = result.Succeeded;
            }

            return RedirectToRoute(SiteRoutes.Home, new { UpdateSuccess = updated });
        }

        private static string FormatErrors(IdentityResult result)
        {
            return string.Join(";", result.Errors.Select((p) => $"{p.Code}: {p.Description}"));
        }

        private async Task<bool> AreLinesValidAsync(UpdateLinePreferencesViewModel model, CancellationToken cancellationToken)
        {
            if (model.FavoriteLines != null)
            {
                ITflService service = _tflServiceFactory.CreateService();
                ICollection<LineInfo> lines = await service.GetLinesAsync(cancellationToken);

                IList<string> validLines = lines.Select((p) => p.Id).ToList();

                return model.FavoriteLines.All((p) => validLines.Contains(p));
            }

            return true;
        }

        private async Task<LondonTravelUser> GetCurrentUserAsync()
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
                    user.RoleClaims.Add(LondonTravelRole.FromClaim(claim));
                    commitUpdate = true;
                }
            }

            if (commitUpdate)
            {
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _telemetry.TrackClaimsUpdated(user.Id);
                }

                return result;
            }
            else
            {
                return IdentityResult.Success;
            }
        }
    }
}
