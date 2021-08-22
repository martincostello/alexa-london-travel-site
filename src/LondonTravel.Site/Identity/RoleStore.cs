// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Identity;

namespace MartinCostello.LondonTravel.Site.Identity;

/// <summary>
/// A class representing a custom implementation of <see cref="IRoleStore{LondonTravelRole}"/>. This class cannot be inherited.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class RoleStore : IRoleStore<LondonTravelRole>
{
    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public Task<IdentityResult> CreateAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IdentityResult> DeleteAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<LondonTravelRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<LondonTravelRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> GetNormalizedRoleNameAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> GetRoleIdAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> GetRoleNameAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task SetNormalizedRoleNameAsync(LondonTravelRole role, string normalizedName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task SetRoleNameAsync(LondonTravelRole role, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IdentityResult> UpdateAsync(LondonTravelRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
