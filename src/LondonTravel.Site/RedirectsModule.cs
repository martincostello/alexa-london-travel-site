// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site;

public static class RedirectsModule
{
    /// <summary>
    /// Gets a random set of annoying YouTube videos.
    /// </summary>
    /// <remarks>
    /// Inspired by <c>https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13</c>.
    /// </remarks>
    private static ReadOnlySpan<string> Videos => new[]
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

    public static IEndpointRouteBuilder MapRedirects(this IEndpointRouteBuilder app)
    {
        (string OldPath, string NewPath)[] redirections =
        [
            ("/register", "/account/register/"),
            ("/sign-out", "/"),
            ("/sign-up", "/account/register/"),
            ("/support", "/help/"),
        ];

        foreach ((string originalPath, string newPath) in redirections)
        {
            Redirect(originalPath, newPath);
        }

        var options = app.ServiceProvider.GetRequiredService<IOptions<SiteOptions>>();

        string[] httpMethods = ["GET", "HEAD", "POST"];

        foreach (string path in options.Value.CrawlerPaths)
        {
            RedirectCrawler(path);
        }

        return app;

        void Redirect(string originalPath, string newPath)
            => app.MapGet(originalPath, () => Results.Redirect(newPath)).ExcludeFromDescription();

        void RedirectCrawler(string path)
        {
            app.MapMethods(
                path,
                httpMethods,
                () => Results.Redirect(Videos[RandomNumberGenerator.GetInt32(0, Videos.Length)]))
                .ExcludeFromDescription();
        }
    }
}
