// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Services;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site;

public static class AlexaModule
{
    public static IEndpointRouteBuilder MapAlexa(this IEndpointRouteBuilder app)
    {
        app.MapGet("/alexa/authorize", async (
            [FromQuery(Name = "state")] string? state,
            [FromQuery(Name = "client_id")] string? clientId,
            [FromQuery(Name = "response_type")] string? responseType,
            [FromQuery(Name = "redirect_uri")] BindableUri? redirectUri,
            ClaimsPrincipal user,
            AlexaService service) =>
        {
            return await service.AuthorizeSkillAsync(
                state,
                clientId,
                responseType,
                redirectUri,
                user);
        })
        .ExcludeFromDescription()
        .RequireAuthorization();

        return app;
    }

    //// TODO Remove workaround for https://github.com/dotnet/aspnetcore/issues/36649 if implemented

    private sealed class BindableUri : Uri
    {
        private BindableUri(string uriString, UriKind uriKind)
            : base(uriString, uriKind)
        {
        }

        public static bool TryParse(string value, out BindableUri? result)
        {
            if (TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
            {
                result = new BindableUri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
                return true;
            }

            result = null;
            return false;
        }
    }
}
