// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/api</c> resource.
    /// </summary>
    [Route("api")]
    public class DocsController : Controller
    {
        /// <summary>
        /// Gets the result for the <c>/api/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/api/</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
