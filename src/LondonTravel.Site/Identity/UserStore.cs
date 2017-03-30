// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Services.Data;

    /// <summary>
    /// A class representing a custom implementation of a user store. This class cannot be inherited.
    /// </summary>
    public sealed class UserStore :
        IUserStore<LondonTravelUser>,
        IUserEmailStore<LondonTravelUser>,
        IUserLoginStore<LondonTravelUser>,
        IUserSecurityStampStore<LondonTravelUser>
    {
        /// <summary>
        /// The <see cref="IDocumentClient"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentClient _client;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
        /// <exception cref="">
        /// <paramref name="client"/> is <see langword="null"/>.
        /// </exception>
        public UserStore(IDocumentClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public async Task AddLoginAsync(LondonTravelUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (user.Logins.Any((p) => p.LoginProvider == login.LoginProvider))
            {
                throw new InvalidOperationException($"Failed to add login for provider '{login.LoginProvider}' for user '{user.Id}' as a login for that provider already exists.");
            }

            user.Logins.Add(LondonTravelLoginInfo.FromUserLoginInfo(login));

            var result = await UpdateAsync(user, cancellationToken);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add login for provider '{login.LoginProvider}' for user '{user.Id}'.");
            }
        }

        /// <inheritdoc />
        public async Task<IdentityResult> CreateAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Id = await _client.CreateAsync(user);

            return IdentityResult.Success;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Id))
            {
                throw new ArgumentException("No user Id specified.", nameof(user));
            }

            bool deleted = await _client.DeleteAsync(user.Id);

            return deleted ?
                IdentityResult.Success :
                ResultForError("UserNotFound", $"User with Id '{user.Id}' does not exist.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public async Task<LondonTravelUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (normalizedEmail == null)
            {
                throw new ArgumentNullException(nameof(normalizedEmail));
            }

            var results = await _client.GetAsync<LondonTravelUser>((p) => p.EmailNormalized == normalizedEmail, cancellationToken);
            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<LondonTravelUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var results = await _client.GetAsync<LondonTravelUser>((p) => p.Id == userId, cancellationToken);
            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<LondonTravelUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }

            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            var results = await _client.GetAsync<LondonTravelUser>(
                (p) => p.Logins.Contains(new LondonTravelLoginInfo() { LoginProvider = loginProvider, ProviderKey = providerKey }),
                cancellationToken);

            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<LondonTravelUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (normalizedUserName == null)
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            var results = await _client.GetAsync<LondonTravelUser>((p) => p.UserNameNormalized == normalizedUserName, cancellationToken);
            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public Task<string> GetEmailAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Email);
        }

        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.EmailConfirmed);
        }

        /// <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IList<UserLoginInfo> logins = user.Logins
                .Select((p) => new UserLoginInfo(p.LoginProvider, p.ProviderKey, p.ProviderDisplayName))
                .ToList();

            return Task.FromResult(logins);
        }

        /// <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.EmailNormalized);
        }

        /// <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserNameNormalized);
        }

        /// <inheritdoc />
        public Task<string> GetSecurityStampAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }

        /// <inheritdoc />
        public Task<string> GetUserIdAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id);
        }

        /// <inheritdoc />
        public Task<string> GetUserNameAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        /// <inheritdoc />
        public Task RemoveLoginAsync(LondonTravelUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }

            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

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
        public Task SetEmailAsync(LondonTravelUser user, string email, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Email = email;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetEmailConfirmedAsync(LondonTravelUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.EmailConfirmed = confirmed;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetNormalizedEmailAsync(LondonTravelUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.EmailNormalized = normalizedEmail;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetNormalizedUserNameAsync(LondonTravelUser user, string normalizedName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserNameNormalized = normalizedName;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetSecurityStampAsync(LondonTravelUser user, string stamp, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetUserNameAsync(LondonTravelUser user, string userName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserName = userName;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(LondonTravelUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.CreatedAt == DateTime.MinValue)
            {
                user.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(user.Timestamp).UtcDateTime;
            }

            LondonTravelUser updated = await _client.ReplaceAsync(user.Id, user, user.ETag);

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

        /// <summary>
        /// Throws an exception if the instance has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UserStore));
            }
        }

        /// <summary>
        /// Creates an <see cref="IdentityResult"/> for the specified error.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>
        /// The created instance of <see cref="IdentityResult"/>.
        /// </returns>
        private IdentityResult ResultForError(string code, string description)
        {
            var error = new IdentityError()
            {
                Code = code,
                Description = description,
            };

            return IdentityResult.Failed(error);
        }
    }
}
