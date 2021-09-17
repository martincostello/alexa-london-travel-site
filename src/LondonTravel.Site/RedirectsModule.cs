// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site;

public static class RedirectsModule
{
    public static IEndpointRouteBuilder MapRedirects(this IEndpointRouteBuilder app)
    {
        // TODO Restore the malicious crawler stuff
        void Redirect(string originalPath, string newPath)
        {
            app.MapGet(originalPath, () => Results.Redirect(newPath)).ExcludeFromDescription();
        }

        var redirections = new (string OldPath, string NewPath)[]
        {
            ("/register", "/account/register/"),
            ("/sign-out", "/"),
            ("/sign-up", "/account/register/"),
            ("/support", "/help/"),
        };

        foreach ((string originalPath, string newPath) in redirections)
        {
            Redirect(originalPath, newPath);
        }

        return app;
    }
}
