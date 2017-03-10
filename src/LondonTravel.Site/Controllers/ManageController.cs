// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Identity;
    using MartinCostello.LondonTravel.Site.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A class representing the controller for the <c>/manage/</c> resource.
    /// </summary>
    [Authorize]
    [Route("manage", Name = SiteRoutes.Manage)]
    public class ManageController : Controller
    {
        private readonly UserManager<LondonTravelUser> _userManager;
        private readonly SignInManager<LondonTravelUser> _signInManager;
        private readonly string _applicationCookieScheme;
        private readonly string _externalCookieScheme;
        private readonly ILogger<ManageController> _logger;

        public ManageController(
          UserManager<LondonTravelUser> userManager,
          SignInManager<LondonTravelUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationCookieScheme = identityCookieOptions.Value.ApplicationCookieAuthenticationScheme;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = loggerFactory.CreateLogger<ManageController>();
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
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                _logger?.LogError($"Failed to get user to manage account.");
                return View("Error");
            }

            var userLogins = await _userManager.GetLoginsAsync(user);

            var otherLogins = _signInManager.GetExternalAuthenticationSchemes()
                .Where((p) => userLogins.All((r) => p.AuthenticationScheme != r.LoginProvider))
                .ToList();

            ViewBag.ShowRemoveButton = userLogins.Count > 1;

            var model = new ManageViewModel()
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            };

            return View(model);
        }

        [HttpPost]
        [Route("link-account", Name = SiteRoutes.LinkAccount)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkAccount(string provider)
        {
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            var redirectUrl = Url.RouteUrl(SiteRoutes.LinkAccountCallback);
            var userId = _userManager.GetUserId(User);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);

            _logger?.LogInformation($"Attempting to link user '{userId}' to provider '{provider}'.");

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("link-account-callback", Name = SiteRoutes.LinkAccountCallback)]
        public async Task<ActionResult> LinkAccountCallback()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                _logger?.LogError($"Failed to get user to link account.");
                return View("Error");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);

            if (info == null)
            {
                _logger?.LogError($"Failed to get external login info for user '{userId}' to link account.");
                return RedirectToRoute(SiteRoutes.Manage, new { Message = SiteMessage.Error });
            }

            _logger.LogInformation($"Adding login for '{info.LoginProvider}' to user '{userId}'.");

            var result = await _userManager.AddLoginAsync(user, info);
            var message = SiteMessage.Error;

            if (result.Succeeded)
            {
                _logger.LogInformation($"Added login for '{info.LoginProvider}' to user '{userId}'.");

                message = SiteMessage.LinkSuccess;

                result = await UpdateClaimsAsync(user, info);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Updated claims for user '{userId}' for provider '{info.LoginProvider}'.");
                }
                else
                {
                    _logger?.LogError($"Failed to update user '{userId}' with additional role claims.");
                }

                await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
            }
            else
            {
                _logger?.LogError(
                    $"Failed to add external login info for user '{userId}': {string.Join(";", result.Errors.Select((p) => $"{p.Code}: {p.Description}"))}.");
            }

            return RedirectToRoute(SiteRoutes.Manage, new { Message = message });
        }

        [HttpPost]
        [Route("remove-account-link", Name = SiteRoutes.RemoveAccountLink)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccountLink(RemoveExternalService account)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var message = SiteMessage.Error;

            if (user != null && account != null)
            {
                _logger.LogInformation($"Removing login for '{account.LoginProvider}' from user '{user.Id}'.");

                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Removed login for '{account.LoginProvider}' from user '{user.Id}'.");

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    message = SiteMessage.RemoveSuccess;
                }
                else
                {
                    _logger?.LogError(
                        $"Failed to remove external login info from user '{user.Id}' for provider '{account.LoginProvider}': {string.Join(";", result.Errors.Select((p) => $"{p.Code}: {p.Description}"))}.");
                }
            }

            return RedirectToRoute(SiteRoutes.Manage, new { Message = message });
        }

        [HttpPost]
        [Route("delete-account", Name = SiteRoutes.DeleteAccount)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                _logger.LogInformation($"Deleting user '{user.Id}'.");

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Deleted user '{user.Id}'.");

                    await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
                    await HttpContext.Authentication.SignOutAsync(_applicationCookieScheme);

                    return RedirectToRoute(SiteRoutes.Home, new { Message = SiteMessage.AccountDeleted });
                }
                else
                {
                    _logger?.LogError(
                        $"Failed to delete user '{user.Id}': {string.Join(";", result.Errors.Select((p) => $"{p.Code}: {p.Description}"))}.");
                }
            }
            else
            {
                _logger?.LogError($"Failed to get user to delete account.");
            }

            return RedirectToRoute(SiteRoutes.Manage, new { Message = SiteMessage.Error });
        }

        private async Task<IdentityResult> UpdateClaimsAsync(LondonTravelUser user, ExternalLoginInfo info)
        {
            if (user.RoleClaims == null)
            {
                user.RoleClaims = new List<LondonTravelRole>();
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
                }
            }

            return await _userManager.UpdateAsync(user);
        }
    }
}
