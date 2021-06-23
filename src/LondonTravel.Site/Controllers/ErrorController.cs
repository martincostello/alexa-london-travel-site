// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Security.Cryptography;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers
{
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
        /// The <see cref="SiteResources"/> to use. This field is read-only.
        /// </summary>
        private readonly SiteResources _resources;

        /// <summary>
        /// The <see cref="ISiteTelemetry"/> to use. This field is read-only.
        /// </summary>
        private readonly ISiteTelemetry _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorController"/> class.
        /// </summary>
        /// <param name="resources">The <see cref="SiteResources"/> to use.</param>
        /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
        public ErrorController(SiteResources resources, ISiteTelemetry telemetry)
        {
            _resources = resources;
            _telemetry = telemetry;
        }

        /// <summary>
        /// Gets the result for the <c>/error</c> action.
        /// </summary>
        /// <param name="id">The optional HTTP status code associated with the error.</param>
        /// <returns>
        /// The result for the <c>/error</c> action.
        /// </returns>
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult Index([FromQuery] int? id)
        {
            int httpCode = id ?? StatusCodes.Status500InternalServerError;

            if (!Enum.IsDefined(typeof(HttpStatusCode), (HttpStatusCode)httpCode) ||
                id < StatusCodes.Status400BadRequest ||
                id > 599)
            {
                httpCode = StatusCodes.Status500InternalServerError;
            }

            string? title = _resources.ErrorTitle;
            string? subtitle = _resources.ErrorSubtitle(httpCode);
            string? message = _resources.ErrorMessage;
            bool isUserError = false;

            switch (httpCode)
            {
                case StatusCodes.Status400BadRequest:
                    title = _resources.ErrorTitle400;
                    subtitle = _resources.ErrorSubtitle400;
                    message = _resources.ErrorMessage400;
                    break;

                case StatusCodes.Status403Forbidden:
                    title = _resources.ErrorTitle403;
                    subtitle = _resources.ErrorSubtitle403;
                    message = _resources.ErrorMessage403;
                    break;

                case StatusCodes.Status405MethodNotAllowed:
                    title = _resources.ErrorTitle405;
                    subtitle = _resources.ErrorSubtitle405;
                    message = _resources.ErrorMessage405;
                    break;

                case StatusCodes.Status404NotFound:
                    title = _resources.ErrorTitle404;
                    subtitle = _resources.ErrorSubtitle404;
                    message = _resources.ErrorMessage404;
                    isUserError = true;
                    break;

                case StatusCodes.Status408RequestTimeout:
                    title = _resources.ErrorTitle408;
                    subtitle = _resources.ErrorSubtitle408;
                    message = _resources.ErrorMessage408;
                    break;

                default:
                    break;
            }

            ViewBag.Title = title;
            ViewBag.Subtitle = subtitle;
            ViewBag.Message = message;
            ViewBag.IsUserError = isUserError;

            Response.StatusCode = httpCode;

            return View("Error", httpCode);
        }

        /// <summary>
        /// Gets the result for various routes that scrapers probe.
        /// </summary>
        /// <returns>
        /// The result for the action.
        /// </returns>
        [HttpGet]
        [HttpHead]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route(".env")]
        [Route(".git/{*catchall}")]
        [Route("admin.php")]
        [Route("admin-console")]
        [Route("admin/login.php")]
        [Route("administrator/index.php")]
        [Route("ajaxproxy/proxy.php")]
        [Route("bitrix/admin/index.php")]
        [Route("blog/{*catchall}")]
        [Route("cms/{*catchall}")]
        [Route("index.php")]
        [Route("invoker/JMXInvokerServlet")]
        [Route("jmx-console/HtmlAdaptor")]
        [Route("license.php")]
        [Route("magmi/web/magmi.php")]
        [Route("manager/index.php")]
        [Route("modules/{*catchall}")]
        [Route("readme.html")]
        [Route("site/{*catchall}")]
        [Route("sites/{*catchall}")]
        [Route("tiny_mce/{*catchall}")]
        [Route("uploadify/{*catchall}")]
        [Route("web-console/Invoker")]
        [Route("wordpress/{*catchall}")]
        [Route("wp/{*catchall}")]
        [Route("wp-admin/{*catchall}")]
        [Route("wp-content/{*catchall}")]
        [Route("wp-includes/{*catchall}")]
        [Route("wp-links-opml.php")]
        [Route("wp-login.php")]
        [Route("xmlrpc.php")]
#pragma warning disable CA5391
        public ActionResult No()
#pragma warning restore CA5391
        {
            _telemetry.TrackSuspiciousCrawler();
            return Redirect(Videos[RandomNumberGenerator.GetInt32(0, Videos.Length)]);
        }
    }
}
