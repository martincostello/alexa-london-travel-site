// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IAccountService _service;

        public RegisterController(IAccountService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets the result for the <c>/account/register/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/account/register/</c> action.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("account/register", Name = SiteRoutes.Register)]
        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoute(SiteRoutes.Home);
            }

            long count = await GetRegisteredUsersCountAsync();

            var model = new RegisterViewModel()
            {
                RegisteredUsers = count,
            };

            return View(model);
        }

        private async Task<long> GetRegisteredUsersCountAsync()
        {
            try
            {
                long count = await _service.GetUserCountAsync(useCache: true);

                // Round down to the nearest thousand.
                // Deduct one for "over X,000 users".
                return ((count - 1) / 1000) * 1000;
            }
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                // Over 7,000 users as of 28/10/2018
                return 7_000;
            }
        }
    }
}
