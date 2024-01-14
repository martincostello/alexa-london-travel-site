// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// A class representing the convention for displaying routes in API documentation. This class cannot be inherited.
/// </summary>
public sealed class ApiExplorerDisplayConvention : IActionModelConvention
{
    /// <inheritdoc />
    public void Apply(ActionModel action)
    {
        action.ApiExplorer.IsVisible =
            action.Attributes.OfType<ProducesAttribute>().Any((p) => p.ContentTypes.Contains(MediaTypeNames.Application.Json)) &&
            !action.Attributes.OfType<ApiExplorerSettingsAttribute>().Any((p) => p.IgnoreApi);
    }
}
