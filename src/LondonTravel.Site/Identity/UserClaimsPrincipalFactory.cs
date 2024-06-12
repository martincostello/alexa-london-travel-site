// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Identity;

/// <summary>
/// A custom implementation of <see cref="UserClaimsPrincipalFactory{LondonTravelUser, LondonTravelRole}"/>
/// that adds additional claims to the principal when it is created by the application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserClaimsPrincipalFactory"/> class.
/// </remarks>
/// <param name="userManager">The <see cref="UserManager{LondonTravelUser}"/> to use.</param>
/// <param name="roleManager">The <see cref="RoleManager{LondonTravelRole}"/> to use.</param>
/// <param name="optionsAccessor">The <see cref="IOptions{IdentityOptions}"/> to use.</param>
public class UserClaimsPrincipalFactory(UserManager<LondonTravelUser> userManager, RoleManager<LondonTravelRole> roleManager, IOptions<IdentityOptions> optionsAccessor) : UserClaimsPrincipalFactory<LondonTravelUser, LondonTravelRole>(userManager, roleManager, optionsAccessor)
{
    /// <inheritdoc />
    public override async Task<ClaimsPrincipal> CreateAsync(LondonTravelUser user)
    {
        var principal = await base.CreateAsync(user);

        if (principal.Identity is ClaimsIdentity identity && user?.RoleClaims != null)
        {
            foreach (var role in user.RoleClaims)
            {
                identity.AddClaim(role.ToClaim());
            }
        }

        return principal;
    }
}
