// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/help/</c> resource.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// The <see cref="UserManager{TUser}"/> to use. This field is read-only.
        /// </summary>
        private readonly UserManager<LondonTravelUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser}"/> to use.</param>
        public HelpController(UserManager<LondonTravelUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the result for the <c>/help/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/help/</c> action.
        /// </returns>
        [HttpGet]
        [Route("help", Name = SiteRoutes.Help)]
        public async Task<IActionResult> Index()
        {
            var model = new HelpViewModel()
            {
                IsSignedIn = User?.Identity?.IsAuthenticated == true,
            };

            if (model.IsSignedIn)
            {
                var user = await _userManager.GetUserAsync(User);

                model.HasFavorites = user?.FavoriteLines?.Count > 0;
                model.IsLinkedToAlexa = !string.IsNullOrEmpty(user?.AlexaToken);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the result for the <c>/support/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/support/</c> action.
        /// </returns>
        [HttpGet]
        [Route("support")]
        public IActionResult Support() => RedirectToRoute(SiteRoutes.Help);
    }
}
