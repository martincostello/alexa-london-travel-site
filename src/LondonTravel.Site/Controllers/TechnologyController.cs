// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers;

/// <summary>
/// A class representing the controller for the <c>/technology/</c> resource.
/// </summary>
[Route("technology", Name = SiteRoutes.Technology)]
public class TechnologyController : Controller
{
    /// <summary>
    /// Gets the result for the <c>/technology/</c> action.
    /// </summary>
    /// <returns>
    /// The result for the <c>/technology/</c> action.
    /// </returns>
    [HttpGet]
    public IActionResult Index() => View();
}
