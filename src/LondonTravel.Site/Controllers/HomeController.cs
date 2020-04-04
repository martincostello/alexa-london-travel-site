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
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services.Tfl;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The <see cref="UserManager{TUser}"/> to use. This field is read-only.
        /// </summary>
        private readonly UserManager<LondonTravelUser> _userManager;

        /// <summary>
        /// The <see cref="ITflServiceFactory"/> to use. This field is read-only.
        /// </summary>
        private readonly ITflServiceFactory _tflFactory;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser}"/> to use.</param>
        /// <param name="tflFactory">The <see cref="ITflServiceFactory"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public HomeController(
            UserManager<LondonTravelUser> userManager,
            ITflServiceFactory tflFactory,
            ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _tflFactory = tflFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/</c> action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// The result for the <c>/</c> action.
        /// </returns>
        [HttpGet]
        [HttpHead]
        [Route("", Name = SiteRoutes.Home)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new LinePreferencesViewModel();

            if (User?.Identity?.IsAuthenticated == true)
            {
                await MapPreferencesAsync(model, cancellationToken);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the result for the <c>/register/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/register/</c> action.
        /// </returns>
        [HttpGet]
        [Route("/register")]
        [Route("/sign-up")]
        public IActionResult Register() => RedirectToRoute(SiteRoutes.Register);

        /// <summary>
        /// Maps the user's preferences onto the specified view model as an asynchronous operation.
        /// </summary>
        /// <param name="model">The view model to map.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to map the view model.
        /// </returns>
        private async Task MapPreferencesAsync(LinePreferencesViewModel model, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger?.LogError($"Failed to get user to render preferences.");
                return;
            }

            model.ETag = user.ETag!;
            model.IsAuthenticated = true;
            model.IsLinkedToAlexa = !string.IsNullOrWhiteSpace(user.AlexaToken);

            ITflService service = _tflFactory.CreateService();
            ICollection<LineInfo> lines = await service.GetLinesAsync(cancellationToken);

            MapFavoriteLines(model, lines, user.FavoriteLines);

            string? updateResult = HttpContext.Request.Query["UpdateSuccess"].FirstOrDefault();

            if (!string.IsNullOrEmpty(updateResult))
            {
                model.UpdateResult = string.Equals(updateResult, bool.TrueString, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Maps the user's favorite lines to the specified view model.
        /// </summary>
        /// <param name="model">The view model to map.</param>
        /// <param name="tflLines">The lines reported by the TfL service.</param>
        /// <param name="userFavorites">The user's favorite lines.</param>
        private void MapFavoriteLines(
            LinePreferencesViewModel model,
            ICollection<LineInfo> tflLines,
            ICollection<string> userFavorites)
        {
            if (tflLines.Count == 0)
            {
                _logger?.LogError($"Failed to map TfL lines as there were no values.");
                return;
            }

            foreach (LineInfo line in tflLines)
            {
                var favorite = new FavoriteLineItem()
                {
                    DisplayName = line.Name!,
                    Id = line.Id!,
                };

                model.AllLines.Add(favorite);
            }

            model.AllLines
                .OrderBy((p) => p.DisplayName, StringComparer.Ordinal)
                .ToList();

            if (userFavorites?.Count > 0)
            {
                foreach (var favorite in model.AllLines)
                {
                    favorite.IsFavorite = userFavorites.Contains(favorite.Id, StringComparer.Ordinal);
                }
            }
        }
    }
}
