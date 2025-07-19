// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using Azure.Security.KeyVault.Secrets;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace MartinCostello.LondonTravel.Site.Identity;

public static partial class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder TryAddAmazon(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "Amazon";

        if (IsProviderEnabled(name, options))
        {
            builder.AddAmazon()
                   .Configure<AmazonAuthenticationOptions>(name);
        }

        return builder;
    }

    public static AuthenticationBuilder TryAddApple(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "Apple";

        if (IsProviderEnabled(name, options))
        {
            builder.AddApple()
                   .Configure<AppleAuthenticationOptions>(name, (providerOptions, serviceProvider) =>
                   {
                       var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                       var secretClient = serviceProvider.GetService<SecretClient>();

                       providerOptions.KeyId = configuration[$"Site:Authentication:ExternalProviders:{name}:KeyId"] ?? string.Empty;
                       providerOptions.TeamId = configuration[$"Site:Authentication:ExternalProviders:{name}:TeamId"] ?? string.Empty;

                       if (secretClient is not null)
                       {
                           providerOptions.GenerateClientSecret = true;
                           providerOptions.PrivateKey = async (keyId, cancellationToken) =>
                           {
                               var secret = await secretClient.GetSecretAsync(
                                   $"AuthKey-{keyId}",
                                   cancellationToken: cancellationToken);

                               return secret.Value.Value.AsMemory();
                           };
                       }
                   });
        }

        return builder;
    }

    public static AuthenticationBuilder TryAddGitHub(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "GitHub";

        if (IsProviderEnabled(name, options))
        {
            builder.AddGitHub()
                   .Configure<GitHubAuthenticationOptions>(
                       name,
                       (p, _) =>
                       {
                           p.Scope.Add("user:email");
                           p.UsePkce = true;
                       });
        }

        return builder;
    }

    public static AuthenticationBuilder TryAddGoogle(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "Google";

        if (IsProviderEnabled(name, options))
        {
            builder.AddGoogle()
                   .Configure<GoogleOptions>(name);
        }

        return builder;
    }

    public static AuthenticationBuilder TryAddMicrosoft(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "Microsoft";

        if (IsProviderEnabled(name, options))
        {
            builder.AddMicrosoftAccount()
                   .Configure<MicrosoftAccountOptions>(name);
        }

        return builder;
    }

    public static AuthenticationBuilder TryAddTwitter(
        this AuthenticationBuilder builder,
        Options.AuthenticationOptions options)
    {
        string name = "Twitter";

        if (IsProviderEnabled(name, options))
        {
            builder.AddTwitter()
                .Services
                .AddOptions<TwitterOptions>(name)
                .Configure<IServiceProvider>((options, serviceProvider) =>
                {
                    var siteOptions = serviceProvider.GetRequiredService<SiteOptions>();
                    var provider = siteOptions!.Authentication!.ExternalProviders![name]!;

                    options.ConsumerKey = provider.ClientId;
                    options.ConsumerSecret = provider.ClientSecret;
                    options.RetrieveUserDetails = true;
                    options.CorrelationCookie.Name = ApplicationCookie.Correlation.Name;
                    options.StateCookie.Name = ApplicationCookie.State.Name;

                    options.Events.OnRemoteFailure =
                        (context) => HandleRemoteFailure(
                            context,
                            options.SignInScheme!,
                            options.StateDataFormat,
                            context.HttpContext.RequestServices.GetRequiredService<ILogger<TwitterOptions>>(),
                            (token) => token?.Properties?.Items);
                });
        }

        return builder;
    }

    public static Task HandleRemoteFailure<T>(
        RemoteFailureContext context,
        string provider,
        ISecureDataFormat<T> secureDataFormat,
        ILogger logger,
        Func<T, IDictionary<string, string?>?> propertiesProvider)
    {
        string? path = GetSiteErrorRedirect(context, secureDataFormat, propertiesProvider);

        if (string.IsNullOrEmpty(path) ||
            !Uri.TryCreate(path, UriKind.Relative, out var notUsed))
        {
            path = "/";
        }

        SiteMessage message;

        if (WasPermissionDenied(context))
        {
            message = SiteMessage.LinkDenied;
            Log.PermissionDenied(logger);
        }
        else
        {
            message = SiteMessage.LinkFailed;
            string errors = string.Join(';', context.Request.Query.Select((p) => $"'{p.Key}' = '{p.Value}'"));

            if (IsCorrelationFailure(context))
            {
                // Not a server-side problem, so do not create log noise
                Log.CorrelationFailed(
                    logger,
                    context.Failure,
                    provider,
                    context.Failure?.Message,
                    errors);
            }
            else
            {
                Log.SignInFailed(
                    logger,
                    context.Failure,
                    provider,
                    context.Failure?.Message,
                    errors);
            }
        }

        context.Response.Redirect($"{path}?Message={message}");
        context.HandleResponse();

        return Task.CompletedTask;
    }

    private static bool IsProviderEnabled(string name, Options.AuthenticationOptions options)
    {
        if (options.ExternalProviders?.TryGetValue(name, out var provider) != true ||
            provider is null)
        {
            return false;
        }

        return provider.IsEnabled;
    }

    private static void Configure<T>(
        this AuthenticationBuilder builder,
        string name,
        Action<T, IServiceProvider>? configure = null)
        where T : OAuthOptions
    {
        builder.Services
            .AddOptions<T>(name)
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                var siteOptions = serviceProvider.GetRequiredService<SiteOptions>();
                var provider = siteOptions!.Authentication!.ExternalProviders![name]!;

                options.ClientId = provider.ClientId!;
                options.ClientSecret = provider.ClientSecret!;
                options.CorrelationCookie.Name = ApplicationCookie.Correlation.Name;

                options.Events.OnRemoteFailure = (context) =>
                {
                    return HandleRemoteFailure(
                        context,
                        context.Scheme.Name,
                        options.StateDataFormat,
                        context.HttpContext.RequestServices.GetRequiredService<ILogger<T>>(),
                        (p) => p?.Items);
                };

                options.Events.OnTicketReceived = (context) =>
                {
                    var provider = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>();

                    context.Properties!.ExpiresUtc = provider.GetUtcNow().AddDays(150);
                    context.Properties.IsPersistent = true;

                    return Task.CompletedTask;
                };

                configure?.Invoke(options, serviceProvider);
            });
    }

    private static string? GetSiteErrorRedirect<T>(
        RemoteFailureContext context,
        ISecureDataFormat<T> secureDataFormat,
        Func<T, IDictionary<string, string?>?> propertiesProvider)
    {
        var state = context.Request.Query["state"];
        var stateData = secureDataFormat.Unprotect(state);
        var properties = propertiesProvider?.Invoke(stateData!);

        if (properties == null ||
            !properties.TryGetValue(SiteContext.ErrorRedirectPropertyName, out string? value))
        {
            value = null;
        }

        return value;
    }

    private static bool WasPermissionDenied(RemoteFailureContext context)
    {
        string? error = context.Request.Query["error"].FirstOrDefault();

        if (string.Equals(error, "access_denied", StringComparison.Ordinal) ||
            string.Equals(error, "consent_required", StringComparison.Ordinal))
        {
            return true;
        }

        string? reason = context.Request.Query["error_reason"].FirstOrDefault();

        if (string.Equals(reason, "user_denied", StringComparison.Ordinal))
        {
            return true;
        }

        string? description = context.Request.Query["error_description"].FirstOrDefault();

        if (!string.IsNullOrEmpty(description) &&
            description.Contains("denied", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return context.Request.Query.ContainsKey("denied");
    }

    private static bool IsCorrelationFailure(RemoteFailureContext context)
    {
        // See https://github.com/aspnet/Security/blob/ad425163b29b1e09a41e84423b0dcbac797c9164/src/Microsoft.AspNetCore.Authentication.OAuth/OAuthHandler.cs#L66
        // and https://github.com/aspnet/Security/blob/2d1c56ce5ccfc15c78dd49cee772f6be473f3ee2/src/Microsoft.AspNetCore.Authentication/RemoteAuthenticationHandler.cs#L203
        // This effectively means that the user did not pass their cookies along correctly to correlate the request.
        return string.Equals(context.Failure?.Message, "Correlation failed.", StringComparison.Ordinal);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Trace,
            Message = "User denied permission.")]
        public static partial void PermissionDenied(ILogger logger);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Trace,
            Message = "Failed to sign-in using {Provider} due to a correlation failure: {FailureMessage}. Errors: {Errors}.")]
        public static partial void CorrelationFailed(ILogger logger, Exception? exception, string provider, string? failureMessage, string errors);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Error,
            Message = "Failed to sign-in using {Provider}: {FailureMessage}. Errors: {Errors}.")]
        public static partial void SignInFailed(ILogger logger, Exception? exception, string provider, string? failureMessage, string errors);
    }
}
