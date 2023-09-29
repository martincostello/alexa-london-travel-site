// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Shared;

[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class Error(SiteResources resources) : PageModel
{
    public int ErrorStatusCode { get; private set; } = StatusCodes.Status500InternalServerError;

    public bool IsClientError { get; private set; }

    public string? Message { get; private set; }

    public string? RequestId { get; private set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? Title { get; private set; }

    public string? Subtitle { get; private set; }

    public void OnGet(int? id = null)
    {
        int httpCode = id ?? StatusCodes.Status500InternalServerError;

        if (!Enum.IsDefined(typeof(HttpStatusCode), (HttpStatusCode)httpCode) ||
            id < StatusCodes.Status400BadRequest ||
            id > 599)
        {
            httpCode = StatusCodes.Status500InternalServerError;
        }

        string? title = resources.ErrorTitle;
        string? subtitle = resources.ErrorSubtitle(httpCode);
        string? message = resources.ErrorMessage;
        bool isUserError = false;

        switch (httpCode)
        {
            case StatusCodes.Status400BadRequest:
                title = resources.ErrorTitle400;
                subtitle = resources.ErrorSubtitle400;
                message = resources.ErrorMessage400;
                break;

            case StatusCodes.Status403Forbidden:
                title = resources.ErrorTitle403;
                subtitle = resources.ErrorSubtitle403;
                message = resources.ErrorMessage403;
                break;

            case StatusCodes.Status405MethodNotAllowed:
                title = resources.ErrorTitle405;
                subtitle = resources.ErrorSubtitle405;
                message = resources.ErrorMessage405;
                break;

            case StatusCodes.Status404NotFound:
                title = resources.ErrorTitle404;
                subtitle = resources.ErrorSubtitle404;
                message = resources.ErrorMessage404;
                isUserError = true;
                break;

            case StatusCodes.Status408RequestTimeout:
                title = resources.ErrorTitle408;
                subtitle = resources.ErrorSubtitle408;
                message = resources.ErrorMessage408;
                break;

            default:
                break;
        }

        Title = title;
        Subtitle = subtitle;
        Message = message;
        IsClientError = isUserError;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        Response.StatusCode = httpCode;
    }
}
