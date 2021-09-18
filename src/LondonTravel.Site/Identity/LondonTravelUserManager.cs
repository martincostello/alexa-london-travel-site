// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Identity;

public sealed class LondonTravelUserManager : AspNetUserManager<LondonTravelUser>
{
    public LondonTravelUserManager(
        IUserStore<LondonTravelUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<LondonTravelUser> passwordHasher,
        IEnumerable<IUserValidator<LondonTravelUser>> userValidators,
        IEnumerable<IPasswordValidator<LondonTravelUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<LondonTravelUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }
}
