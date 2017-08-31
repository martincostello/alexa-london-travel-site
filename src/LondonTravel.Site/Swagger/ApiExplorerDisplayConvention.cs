// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    /// <summary>
    /// A class representing the convention for displaying routes in API documentation. This class cannot be inherited.
    /// </summary>
    public sealed class ApiExplorerDisplayConvention : IActionModelConvention
    {
        /// <inheritdoc />
        public void Apply(ActionModel action)
        {
            action.ApiExplorer.IsVisible =
                action.Attributes.OfType<ProducesAttribute>().Any((p) => p.ContentTypes.Contains("application/json")) &&
                !action.Attributes.OfType<ApiExplorerSettingsAttribute>().Any((p) => p.IgnoreApi);
        }
    }
}
