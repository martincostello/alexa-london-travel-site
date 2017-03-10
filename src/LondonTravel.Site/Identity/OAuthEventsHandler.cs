// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class that extends <see cref="OAuthEvents"/> to allow for custom error handling.
    /// </summary>
    public class OAuthEventsHandler : OAuthEvents
    {
        /// <summary>
        /// The authentication provider assocaited with the instance. This field is read-only.
        /// </summary>
        private readonly string _provider;

        /// <summary>
        /// The <see cref="IOAuthEvents"/> wrapped by this instance for events it does not handle itself. This field is read-only.
        /// </summary>
        private readonly IOAuthEvents _wrapped;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<OAuthEventsHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthEventsHandler"/> class.
        /// </summary>
        /// <param name="options">The <see cref="OAuthOptions"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        public OAuthEventsHandler(OAuthOptions options, ILoggerFactory loggerFactory)
        {
            _provider = options.AuthenticationScheme;
            _wrapped = options.Events;
            _logger = loggerFactory.CreateLogger<OAuthEventsHandler>();

            // Setup custom handler
            OnRemoteFailure = HandleRemoteFailure;

            // Assign delegated handlers
            OnCreatingTicket = _wrapped.CreatingTicket;
            OnRedirectToAuthorizationEndpoint = _wrapped.RedirectToAuthorizationEndpoint;
            OnTicketReceived = _wrapped.TicketReceived;
        }

        /// <summary>
        /// Handles a remote failure.
        /// </summary>
        /// <param name="context">The failure context.</param>
        /// <param name="provider">The authentication provider.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        internal static Task HandleRemoteFailure(FailureContext context, string provider, ILogger logger)
        {
            string errors = string.Join(";", context.Request.Query.Select((p) => $"'{p.Key}' = '{p.Value}'"));
            logger?.LogError(default(EventId), context.Failure, $"Failed to sign-in using '{provider}': '{context.Failure.Message}'. Errors: {errors}.");

            SiteMessage message;

            if (string.Equals(context.Request.Query["error"].FirstOrDefault(), "access_denied", StringComparison.Ordinal) ||
                string.Equals(context.Request.Query["error_reason"].FirstOrDefault(), "user_denied", StringComparison.Ordinal))
            {
                message = SiteMessage.LinkDenied;
            }
            else
            {
                message = SiteMessage.LinkFailed;
            }

            context.Response.Redirect($"/account/sign-in/?Message={message}");
            context.HandleResponse();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles a remote failure.
        /// </summary>
        /// <param name="context">The failure context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        private async Task HandleRemoteFailure(FailureContext context)
        {
            try
            {
                await HandleRemoteFailure(context, _provider, _logger);
            }
            catch (Exception ex)
            {
                _logger?.LogError(default(EventId), ex, $"Failed to handle remote failure: {ex.Message}.");

                if (!context.HandledResponse)
                {
                    await _wrapped.RemoteFailure(context);
                }
            }
        }
    }
}
