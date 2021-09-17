// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site;

public static class RedirectsModule
{
    public static IEndpointRouteBuilder MapRedirects(this IEndpointRouteBuilder app)
    {
        app.MapGet("/register", () => Results.Redirect("/account/register/"));
        app.MapGet("/sign-out", () => Results.Redirect("/"));
        app.MapGet("/sign-up", () => Results.Redirect("/account/register/"));
        app.MapGet("/support", () => Results.Redirect("/help/"));

        return app;
    }
}
