// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Services.Data;
using Microsoft.AspNetCore.Identity;

namespace MartinCostello.LondonTravel.Site.Identity;

/// <summary>
/// A class representing a custom implementation of a user store. This class cannot be inherited.
/// </summary>
public sealed class UserStore :
    IUserStore<LondonTravelUser>,
    IUserEmailStore<LondonTravelUser>,
    IUserLoginStore<LondonTravelUser>,
    IUserRoleStore<LondonTravelUser>,
    IUserSecurityStampStore<LondonTravelUser>
{
    /// <summary>
    /// The issuer to use for roles.
    /// </summary>
    private const string RoleClaimIssuer = "https://londontravel.martincostello.com/";

    /// <summary>
    /// The <see cref="IDocumentService"/> to use. This field is read-only.
    /// </summary>
    private readonly IDocumentService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStore"/> class.
    /// </summary>
    /// <param name="service">The <see cref="IDocumentService"/> to use.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="service"/> is <see langword="null"/>.
    /// </exception>
    public UserStore(IDocumentService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    /// <inheritdoc />
    public Task AddLoginAsync(LondonTravelUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(login);

        if (user.Logins.Any((p) => p.LoginProvider == login.LoginProvider))
        {
            throw new InvalidOperationException($"Failed to add login for provider '{login.LoginProvider}' for user '{user.Id}' as a login for that provider already exists.");
        }

        user.Logins.Add(LondonTravelLoginInfo.FromUserLoginInfo(login));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> CreateAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Id = await _service.CreateAsync(user);

        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> DeleteAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("No user Id specified.", nameof(user));
        }

        bool deleted = await _service.DeleteAsync(user.Id);

        return deleted ?
            IdentityResult.Success :
            ResultForError("UserNotFound", $"User with Id '{user.Id}' does not exist.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Not used
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedEmail);

        var results = await _service.GetAsync((p) => p.EmailNormalized == normalizedEmail, cancellationToken);
        var result = results.FirstOrDefault();
        return result!;
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var user = await _service.GetAsync(userId);
        return user!;
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loginProvider);
        ArgumentNullException.ThrowIfNull(providerKey);

        var results = await _service.GetAsync(
            (p) => p.Logins.Contains(new LondonTravelLoginInfo() { LoginProvider = loginProvider, ProviderKey = providerKey, ProviderDisplayName = null }) ||
                   p.Logins.Contains(new LondonTravelLoginInfo() { LoginProvider = loginProvider, ProviderKey = providerKey, ProviderDisplayName = loginProvider }),
            cancellationToken);

        var result = results.FirstOrDefault();
        return result!;
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedUserName);

        var results = await _service.GetAsync((p) => p.UserNameNormalized == normalizedUserName, cancellationToken);
        var result = results.FirstOrDefault();
        return result!;
    }

    /// <inheritdoc />
    public Task<string?> GetEmailAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.Email);
    }

    /// <inheritdoc />
    public Task<bool> GetEmailConfirmedAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.EmailConfirmed);
    }

    /// <inheritdoc />
    public Task<IList<UserLoginInfo>> GetLoginsAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        IList<UserLoginInfo> logins = user.Logins
            .Select((p) => new UserLoginInfo(p.LoginProvider!, p.ProviderKey!, p.ProviderDisplayName))
            .ToList();

        return Task.FromResult(logins);
    }

    /// <inheritdoc />
    public Task<string?> GetNormalizedEmailAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.EmailNormalized);
    }

    /// <inheritdoc />
    public Task<string?> GetNormalizedUserNameAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.UserNameNormalized);
    }

    /// <inheritdoc />
    public Task<string?> GetSecurityStampAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.SecurityStamp);
    }

    /// <inheritdoc />
    public Task<string> GetUserIdAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.Id!);
    }

    /// <inheritdoc />
    public Task<string?> GetUserNameAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.UserName);
    }

    /// <inheritdoc />
    public Task RemoveLoginAsync(LondonTravelUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(loginProvider);
        ArgumentNullException.ThrowIfNull(providerKey);

        if (user.Logins != null)
        {
            var loginsToRemove = user.Logins
                .Where((p) => p.LoginProvider == loginProvider)
                .Where((p) => p.ProviderKey == providerKey)
                .ToList();

            if (loginsToRemove.Count > 0)
            {
                foreach (var login in loginsToRemove)
                {
                    user.Logins.Remove(login);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetEmailAsync(LondonTravelUser user, string? email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Email = email;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetEmailConfirmedAsync(LondonTravelUser user, bool confirmed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetNormalizedEmailAsync(LondonTravelUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.EmailNormalized = normalizedEmail;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetNormalizedUserNameAsync(LondonTravelUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.UserNameNormalized = normalizedName;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetSecurityStampAsync(LondonTravelUser user, string? stamp, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetUserNameAsync(LondonTravelUser user, string? userName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.UserName = userName;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> UpdateAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.CreatedAt == DateTime.MinValue)
        {
            user.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(user.Timestamp).UtcDateTime;
        }

        var updated = await _service.ReplaceAsync(user, user.ETag);

        if (updated != null)
        {
            user.ETag = updated.ETag;
            return IdentityResult.Success;
        }
        else
        {
            return ResultForError("Conflict", "The user could not be updated as the ETag value is out-of-date.");
        }
    }

    /// <inheritdoc />
    public Task AddToRoleAsync(LondonTravelUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IList<string>> GetRolesAsync(LondonTravelUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        IList<string> roles = [];

        if (user.RoleClaims?.Count > 0)
        {
            roles = user.RoleClaims
                .Where((p) => string.Equals(p.Issuer, RoleClaimIssuer, StringComparison.Ordinal))
                .Where((p) => string.Equals(p.ClaimType, ClaimTypes.Role, StringComparison.Ordinal))
                .Where((p) => string.Equals(p.ValueType, ClaimValueTypes.String, StringComparison.Ordinal))
                .Where((p) => !string.IsNullOrEmpty(p.Value))
                .Select((p) => p.Value!)
                .ToArray();
        }

        return Task.FromResult(roles);
    }

    /// <inheritdoc />
    public Task<IList<LondonTravelUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> IsInRoleAsync(LondonTravelUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roleName);

        bool isInRole = false;

        if (user.RoleClaims?.Count > 0)
        {
            isInRole = user.RoleClaims
                .Where((p) => string.Equals(p.Issuer, RoleClaimIssuer, StringComparison.Ordinal))
                .Where((p) => string.Equals(p.ClaimType, ClaimTypes.Role, StringComparison.Ordinal))
                .Where((p) => string.Equals(p.ValueType, ClaimValueTypes.String, StringComparison.Ordinal))
                .Any((p) => string.Equals(p.Value, roleName, StringComparison.Ordinal));
        }

        return Task.FromResult(isInRole);
    }

    /// <inheritdoc />
    public Task RemoveFromRoleAsync(LondonTravelUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates an <see cref="IdentityResult"/> for the specified error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>
    /// The created instance of <see cref="IdentityResult"/>.
    /// </returns>
    private static IdentityResult ResultForError(string code, string description)
    {
        var error = new IdentityError()
        {
            Code = code,
            Description = description,
        };

        return IdentityResult.Failed(error);
    }
}
