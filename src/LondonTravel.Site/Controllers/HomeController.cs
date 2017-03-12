// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Tfl;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The <see cref="ITflServiceFactory"/> to use. This field is read-only.
        /// </summary>
        private readonly ITflServiceFactory _tflFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="tflFactory">The <see cref="ITflServiceFactory"/> to use.</param>
        public HomeController(ITflServiceFactory tflFactory)
        {
            _tflFactory = tflFactory;
        }

        /// <summary>
        /// Gets the result for the <c>/</c> action.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// The result for the <c>/</c> action.
        /// </returns>
        [HttpGet]
        [Route("", Name = SiteRoutes.Home)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            IList<Tuple<string, string>> lineInfo = Array.Empty<Tuple<string, string>>();

            if (User.Identity.IsAuthenticated)
            {
                using (var client = _tflFactory.CreateService())
                {
                    var lines = await client.GetLinesAsync(cancellationToken);

                    var lineNames = new List<Tuple<string, string>>(lines.Count);

                    foreach (JObject line in lines)
                    {
                        lineNames.Add(Tuple.Create((string)line["id"], (string)line["name"]));
                    }

                    lineInfo = lineNames
                        .OrderBy((p) => p.Item2)
                        .ToList();
                }
            }

            return View(lineInfo);
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
    }
}
