// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;

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

        string[] crawlerPaths =
        [
            ".env",
            ".git/{*catchall}",
            "admin.php",
            "admin-console/{*catchall}",
            "admin/{*catchall}",
            "administration/{*catchall}",
            "administrator/{*catchall}",
            "appsettings.json",
            "ajaxproxy/{*catchall}",
            "bin/{*catchall}",
            "bitrix/admin/{*catchall}",
            "blog/{*catchall}",
            "cms/{*catchall}",
            "index.php",
            "invoker/JMXInvokerServlet",
            "jmx-console/HtmlAdaptor",
            "license.php",
            "magmi/web/magmi.php",
            "manager/index.php",
            "modules/{*catchall}",
            "obj/{*catchall}",
            "package.json",
            "package-lock.json",
            "parameters.xml",
            "readme.htm",
            "readme.html",
            "site/{*catchall}",
            "sites/{*catchall}",
            "tiny_mce/{*catchall}",
            "uploadify/{*catchall}",
            "web.config",
            "web-console/Invoker",
            "wordpress/{*catchall}",
            "wp/{*catchall}",
            "wp-admin/{*catchall}",
            "wp-content/{*catchall}",
            "wp-includes/{*catchall}",
            "wp-links-opml.php",
            "wp-login.php",
            "xmlrpc.php",
        ];

        string[] httpMethods = ["GET", "HEAD", "POST"];

        foreach (string path in crawlerPaths)
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
