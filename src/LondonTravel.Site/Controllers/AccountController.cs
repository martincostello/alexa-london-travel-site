// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Identity;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A class representing the controller for the <c>/account/</c> resource.
    /// </summary>
    [Authorize]
    [Route("account", Name = SiteRoutes.Account)]
    public class AccountController : Controller
    {
        private readonly UserManager<LondonTravelUser> _userManager;
        private readonly SignInManager<LondonTravelUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly bool _isEnabled;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<LondonTravelUser> userManager,
            SignInManager<LondonTravelUser> signInManager,
            IOptionsSnapshot<IdentityCookieOptions> identityCookieOptions,
            SiteOptions siteOptions,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = logger;

            _isEnabled =
                siteOptions?.Authentication?.IsEnabled == true &&
                siteOptions?.Authentication.ExternalProviders?.Any((p) => p.Value?.IsEnabled == true) == true;
        }

        /// <summary>
        /// Gets the result for the <c>/account/access-denied/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/account/access-denied/</c> action.
        /// </returns>
        [HttpGet]
        [Route("access-denied", Name = SiteRoutes.AccessDenied)]
        public IActionResult AccessDenied() => View();

        /// <summary>
        /// Gets the result for the <c>/account/sign-in/</c> action.
        /// </summary>
        /// <param name="returnUrl">The optional return URL once the user is signed-in.</param>
        /// <returns>
        /// The result for the <c>/account/sign-in/</c> action.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("sign-in", Name = SiteRoutes.SignIn)]
        public async Task<IActionResult> SignIn(string returnUrl = null)
        {
            if (!_isEnabled)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToRoute(SiteRoutes.Home);
            }

            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Gets the result for the GET <c>/account/sign-out/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/account/sign-out/</c> action.
        /// </returns>
        [HttpGet]
        [Route("sign-out", Name = SiteRoutes.SignOut)]
        [ValidateAntiForgeryToken]
        public IActionResult SignOutGet() => RedirectToRoute(SiteRoutes.Home);

        /// <summary>
        /// Gets the result for the POST <c>/account/sign-out/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/account/sign-out/</c> action.
        /// </returns>
        [HttpPost]
        [Route("sign-out", Name = SiteRoutes.SignOut)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOutPost()
        {
            if (!_isEnabled)
            {
                return NotFound();
            }

            string userName = _userManager.GetUserName(User);

            await _signInManager.SignOutAsync();

            _logger.LogInformation($"User '{userName}' signed out.");

            return RedirectToRoute(SiteRoutes.Home);
        }

        /// <summary>
        /// Gets the result for the <c>/account/external-sign-in/</c> action.
        /// </summary>
        /// <param name="provider">The external provider name.</param>
        /// <param name="returnUrl">The optional return URL once the user is signed-in.</param>
        /// <returns>
        /// The result for the <c>/account/external-sign-in/</c> action.
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("external-sign-in", Name = SiteRoutes.ExternalSignIn)]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalSignIn(string provider, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            if (!_isEnabled)
            {
                return NotFound();
            }

            var redirectUrl = Url.RouteUrl(SiteRoutes.ExternalSignInCallback, new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            string errorRedirectUrl = Url.RouteUrl(IsReferrerRegistrationPage() ? SiteRoutes.Register : SiteRoutes.SignIn);
            SiteContext.SetErrorRedirect(properties, errorRedirectUrl);

            return Challenge(properties, provider);
        }

        /// <summary>
        /// Gets the result for the <c>/account/external-sign-in-callback/</c> action.
        /// </summary>
        /// <param name="returnUrl">The optional return URL once the user is signed-in.</param>
        /// <param name="remoteError">The remote error message, if any.</param>
        /// <returns>
        /// The result for the <c>/account/external-sign-in-callback/</c> action.
        /// </returns>
        [AllowAnonymous]
        [Route("external-sign-in-callback", Name = SiteRoutes.ExternalSignInCallback)]
        [HttpGet]
        public async Task<IActionResult> ExternalSignInCallback(string returnUrl = null, string remoteError = null)
        {
            if (!_isEnabled)
            {
                return NotFound();
            }

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(SignIn));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToRoute(SiteRoutes.SignIn);
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User '{_userManager.GetUserName(info.Principal)}' signed in with '{info.LoginProvider}' provider.");

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return View("LockedOut");
            }
            else
            {
                LondonTravelUser user = CreateSystemUser(info);

                if (string.IsNullOrEmpty(user?.Email))
                {
                    ViewData["Message"] = nameof(SiteMessage.PermissionDenied);
                    ViewData["ReturnUrl"] = returnUrl;

                    return View("SignIn");
                }

                var identityResult = await _userManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);

                    _logger.LogInformation($"New user account '{user.Id}' created through '{info.LoginProvider}'.");

                    return RedirectToRoute(SiteRoutes.Home, new { Message = SiteMessage.AccountCreated });
                }

                bool isUserAlreadyRegistered = identityResult.Errors.Any((p) => p.Code.StartsWith("Duplicate"));

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

        /// <summary>
        /// Gets the result for the <c>/account/register/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/account/register/</c> action.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("register", Name = SiteRoutes.Register)]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToRoute(SiteRoutes.Home);
            }

            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                _logger?.LogWarning($"{error.Code}: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute(SiteRoutes.Home);
            }
        }

        private LondonTravelUser CreateSystemUser(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

            var user = new LondonTravelUser()
            {
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

        private bool IsReferrerRegistrationPage()
        {
            string referrer = HttpContext.Request.Headers["referer"];

            if (string.IsNullOrWhiteSpace(referrer) ||
                !Uri.TryCreate(referrer, UriKind.Absolute, out Uri referrerUri))
            {
                return false;
            }

            string registerUrl = Url.RouteUrl(SiteRoutes.Register);

            return string.Equals(referrerUri.AbsolutePath, registerUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}
