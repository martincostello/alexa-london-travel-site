// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Models;
    using MartinCostello.LondonTravel.Site.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class RegisterController : Controller
    {
        private readonly IAccountService _service;
        private readonly ILogger _logger;

        public RegisterController(IAccountService service, ILogger<RegisterController> logger)
        {
            _service = service;
            _logger = logger;
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
            if (User.Identity.IsAuthenticated)
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
            catch (Exception)
            {
                // Over 6,000 users as of 21/10/2018
                return 6000;
            }
        }
    }
}
