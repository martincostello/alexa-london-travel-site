// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/error</c> resource.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// A random set of annoying YouTube videos. This field is read-only.
        /// </summary>
        /// <remarks>
        /// Inspired by <c>https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13</c>.
        /// </remarks>
        private static readonly string[] Videos =
        {
            "https://www.youtube.com/watch?v=wbby9coDRCk",
            "https://www.youtube.com/watch?v=nb2evY0kmpQ",
            "https://www.youtube.com/watch?v=eh7lp9umG2I",
            "https://www.youtube.com/watch?v=z9Uz1icjwrM",
            "https://www.youtube.com/watch?v=Sagg08DrO5U",
            "https://www.youtube.com/watch?v=ER97mPHhgtM",
            "https://www.youtube.com/watch?v=jI-kpVh6e1U",
            "https://www.youtube.com/watch?v=jScuYd3_xdQ",
            "https://www.youtube.com/watch?v=S5PvBzDlZGs",
            "https://www.youtube.com/watch?v=9UZbGgXvCCA",
            "https://www.youtube.com/watch?v=O-dNDXUt1fg",
            "https://www.youtube.com/watch?v=MJ5JEhDy8nE",
            "https://www.youtube.com/watch?v=VnnWp_akOrE",
            "https://www.youtube.com/watch?v=jwGfwbsF4c4",
            "https://www.youtube.com/watch?v=8ZcmTl_1ER8",
            "https://www.youtube.com/watch?v=gLmcGkvJ-e0",
            "https://www.youtube.com/watch?v=ozPPwl53c_4",
            "https://www.youtube.com/watch?v=KMFOVSWn0mI",
            "https://www.youtube.com/watch?v=clU0Sh9ngmY",
            "https://www.youtube.com/watch?v=sCNrK-n68CM",
            "https://www.youtube.com/watch?v=hgwpZvTWLmE",
            "https://www.youtube.com/watch?v=CgBJ5irINqU",
            "https://www.youtube.com/watch?v=jAckVuEY_Rc",
        };

        /// <summary>
        /// Gets the result for the <c>/error</c> action.
        /// </summary>
        /// <param name="id">The optional HTTP status code associated with the error.</param>
        /// <returns>
        /// The result for the <c>/error</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index(int? id) => View("Error", id ?? 500);

        /// <summary>
        /// Gets the result for various routes that scrapers probe.
        /// </summary>
        /// <returns>
        /// The result for the action.
        /// </returns>
        [Route("admin.php")]
        [Route("admin/login.php")]
        [Route("administrator/index.php")]
        [Route("ajaxproxy/proxy.php")]
        [Route("bitrix/admin/index.php")]
        [Route("index.php")]
        [Route("magmi/web/magmi.php")]
        [Route("wp-admin/admin-ajax.php")]
        [Route("wp-admin/includes/themes.php")]
        [Route("wp-admin/options-link.php")]
        [Route("wp-admin/post-new.php")]
        [Route("wp-login.php")]
        [Route("xmlrpc.php")]
        public ActionResult No() => Redirect(Videos[new Random().Next(0, Videos.Length)]);
    }
}
