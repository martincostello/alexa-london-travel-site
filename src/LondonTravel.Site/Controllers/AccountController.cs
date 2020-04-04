// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using Options;
    using Telemetry;

    /// <summary>
    /// A class representing the controller for the <c>/account/</c> resource.
    /// </summary>
    [Authorize]
    [Route("account", Name = SiteRoutes.Account)]
    public class AccountController : Controller
    {
        /// <summary>
        /// The names of the authentication schemes that are disallowed for
        /// sign-in to link Alexa to an account. This field is read-only.
        /// </summary>
        private static readonly string[] AuthenticationSchemesDisabledForAlexa = new[] { "google" };

        //// TODO Move more of the implementation into IAccountService

        private readonly UserManager<LondonTravelUser> _userManager;
        private readonly SignInManager<LondonTravelUser> _signInManager;
        private readonly ISiteTelemetry _telemetry;
        private readonly IClock _clock;
        private readonly bool _isEnabled;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<LondonTravelUser> userManager,
            SignInManager<LondonTravelUser> signInManager,
            ISiteTelemetry telemetry,
            IClock clock,
            SiteOptions siteOptions,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _telemetry = telemetry;
            _clock = clock;
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
        public async Task<IActionResult> SignIn(string? returnUrl = null)
        {
            if (!_isEnabled)
            {
                return NotFound();
            }

            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoute(SiteRoutes.Home);
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;

            string viewName = nameof(SignIn);

            if (IsRedirectAlexaAuthorization(returnUrl))
            {
                viewName += "Alexa";
                ViewData["AuthenticationSchemesToHide"] = AuthenticationSchemesDisabledForAlexa;
            }

            return View(viewName);
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

            string userId = _userManager.GetUserId(User);

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User Id {UserId} signed out.", userId);

            _telemetry.TrackSignOut(userId);

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

            var redirectUrl = Url.RouteUrl(SiteRoutes.ExternalSignInCallback, new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            string errorRedirectUrl = GetErrorRedirectUrl();
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
        public async Task<IActionResult> ExternalSignInCallback(string? returnUrl = null, string? remoteError = null)
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
                string userId = _userManager.GetUserId(info.Principal);

                _logger.LogInformation("User Id {UserId} signed in with provider {LoginProvider}.", userId, info.LoginProvider);
                _telemetry.TrackSignIn(userId, info.LoginProvider);

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return View("LockedOut");
            }
            else
            {
                LondonTravelUser? user = CreateSystemUser(info);

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

                    _logger.LogInformation("New user account {UserId} created through {LoginProvider}.", user.Id, info.LoginProvider);

                    _telemetry.TrackAccountCreated(user.Id!, user.Email, info.LoginProvider);

                    if (IsRedirectAlexaAuthorization(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToRoute(SiteRoutes.Home, new { Message = SiteMessage.AccountCreated });
                    }
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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                _logger?.LogWarning("{ErrorCode}: {ErrorDescription}", error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
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

        private LondonTravelUser? CreateSystemUser(ExternalLoginInfo info)
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
                CreatedAt = _clock.GetCurrentInstant().ToDateTimeUtc(),
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
            return Url.RouteUrl(IsReferrerRegistrationPage() ? SiteRoutes.Register : SiteRoutes.SignIn);
        }

        private bool IsReferrerRegistrationPage() => IsReferrerRoute(SiteRoutes.Register);

        private bool IsRedirectAlexaAuthorization(string? returnUrl) => IsUrlRoute(returnUrl, SiteRoutes.AuthorizeAlexa);

        private bool IsReferrerRoute(string routeName)
        {
            return IsUrlRoute(HttpContext.Request.Headers["referer"], routeName);
        }

        private bool IsUrlRoute(string? url, string routeName)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                return false;
            }

            string routeUrl = Url.RouteUrl(routeName);

            if (uri.IsAbsoluteUri)
            {
                return string.Equals(uri.AbsolutePath, routeUrl, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                int indexOfQuery = url.IndexOf('?', StringComparison.Ordinal);

                if (indexOfQuery > -1)
                {
                    url = url.Substring(0, indexOfQuery);
                }

                var toTrim = new[] { '/' };

                return string.Equals(url.TrimEnd(toTrim), routeUrl.TrimEnd(toTrim), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
