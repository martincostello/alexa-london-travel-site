// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class Index(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    ILogger<Index> logger) : PageModel
{
    /// <summary>
    /// Gets the external logins currently linked to the user.
    /// </summary>
    public IList<UserLoginInfo> CurrentLogins { get; private set; } = [];

    /// <summary>
    /// Gets the external logins that can be linked to the user.
    /// </summary>
    public IList<AuthenticationScheme> OtherLogins { get; private set; } = [];

    /// <summary>
    /// Gets a value indicating whether the user can link more external logins.
    /// </summary>
    public bool CanAddMoreLogins => OtherLogins.Count > 0;

    /// <summary>
    /// Gets the ETag associated with the user.
    /// </summary>
    public string ETag { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the user is linked to the Alexa app.
    /// </summary>
    public bool IsLinkedToAlexa { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to show the buttons to remove external logins.
    /// </summary>
    public bool ShowRemoveButton => CurrentLogins.Count > 1;

    public async Task<IActionResult> OnGet()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            Log.FailedToGetUserToManageAccount(logger);
            return RedirectToPage(SiteRoutes.Error);
        }

        var userLogins = await userManager.GetLoginsAsync(user);

        var otherLogins = (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Where((p) => userLogins.All((r) => p.Name != r.LoginProvider))
            .OrderBy((p) => p.DisplayName)
            .ThenBy((p) => p.Name)
            .ToList();

        foreach (var login in userLogins)
        {
            login.ProviderDisplayName ??= login.LoginProvider;
        }

        CurrentLogins = [.. userLogins.OrderBy((p) => p.ProviderDisplayName).ThenBy((p) => p.LoginProvider)];
        ETag = user.ETag!;
        IsLinkedToAlexa = !string.IsNullOrWhiteSpace(user.AlexaToken);
        OtherLogins = otherLogins;

        return Page();
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Failed to get user to manage account.")]
        public static partial void FailedToGetUserToManageAccount(ILogger logger);
    }
}
