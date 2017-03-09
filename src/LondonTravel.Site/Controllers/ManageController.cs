// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
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
        private readonly string _externalCookieScheme;

        public ManageController(
          UserManager<LondonTravelUser> userManager,
          SignInManager<LondonTravelUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
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

            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("link-account-callback", Name = SiteRoutes.LinkAccountCallback)]
        public async Task<ActionResult> LinkAccountCallback()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return View("Error");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);

            if (info == null)
            {
                return RedirectToAction(nameof(Index), new { Message = "Error" });
            }

            var result = await _userManager.AddLoginAsync(user, info);

            var message = "Error";

            if (result.Succeeded)
            {
                message = "LinkSuccess";

                await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
            }

            return RedirectToAction(nameof(Index), new { Message = message });
        }

        [HttpPost]
        [Route("remove-account", Name = SiteRoutes.RemoveAccount)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccount(RemoveExternalService account)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var message = "Error";

            if (user != null && account != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    message = "RemoveSuccess";
                }
            }

            return RedirectToAction(nameof(Index), new { Message = message });
        }
    }
}
