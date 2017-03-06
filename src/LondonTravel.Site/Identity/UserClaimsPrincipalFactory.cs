// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// A custom implementation of <see cref="UserClaimsPrincipalFactory{LondonTravelUser, LondonTravelRole}"/>
    /// that adds additional claims to the principal when it is created by the application.
    /// </summary>
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<LondonTravelUser, LondonTravelRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{LondonTravelUser}"/> to use.</param>
        /// <param name="roleManager">The <see cref="RoleManager{LondonTravelRole}"/> to use.</param>
        /// <param name="optionsAccessor">The <see cref="IOptions{IdentityOptions}"/> to use.</param>
        public UserClaimsPrincipalFactory(UserManager<LondonTravelUser> userManager, RoleManager<LondonTravelRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        /// <inheritdoc />
        public async override Task<ClaimsPrincipal> CreateAsync(LondonTravelUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = principal.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.GivenName, user.GivenName ?? string.Empty),
                    new Claim(ClaimTypes.Surname, user.Surname ?? string.Empty),
                };

                identity.AddClaims(claims);
            }

            return principal;
        }
    }
}
