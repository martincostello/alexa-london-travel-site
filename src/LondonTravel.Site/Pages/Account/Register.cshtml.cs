// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class Register : PageModel
{
    private readonly IAccountService _service;

    public Register(IAccountService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets or sets the approximate number of registered users.
    /// </summary>
    public long RegisteredUsers { get; set; }

    public async Task OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            RedirectToRoute(SiteRoutes.Home);
            return;
        }

        long count = await GetRegisteredUsersCountAsync();

        RegisteredUsers = count;
    }

    private async Task<long> GetRegisteredUsersCountAsync()
    {
        try
        {
            long count = await _service.GetUserCountAsync(useCache: true);

            // Round down to the nearest thousand.
            // Deduct one for "over X,000 users".
            return ((count - 1) / 1000) * 1000;
        }
        catch (Exception)
        {
            // Over 9,500 users as of 22/08/2021
            return 9_500;
        }
    }
}
