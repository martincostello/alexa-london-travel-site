// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Telemetry;

/// <summary>
/// A class containing extension methods for the <see cref="ISiteTelemetry"/> interface. This class cannot be inherited.
/// </summary>
public static class ISiteTelemetryExtensions
{
    /// <summary>
    /// Tracks an account being created.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="provider">The provider used to create the account.</param>
    public static void TrackAccountCreated(this ISiteTelemetry telemetry, string userId, string emailAddress, string provider)
    {
        var properties = CreateProperties(userId, provider);
        properties["EmailAddress"] = emailAddress;

        telemetry.TrackEvent("AccountCreated", properties);
    }

    /// <summary>
    /// Tracks an account being deleted.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="emailAddress">The email address.</param>
    public static void TrackAccountDeleted(this ISiteTelemetry telemetry, string userId, string emailAddress)
    {
        var properties = CreatePropertiesForUser(userId);
        properties["EmailAddress"] = emailAddress;

        telemetry.TrackEvent("AccountDeleted", properties);
    }

    /// <summary>
    /// Tracks a successful request to link the Alexa app.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    public static void TrackAlexaLink(this ISiteTelemetry telemetry, string userId)
    {
        var properties = CreatePropertiesForUser(userId);

        telemetry.TrackEvent("AlexaLink", properties);
    }

    /// <summary>
    /// Tracks a successful API request for preferences.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    public static void TrackApiPreferencesSuccess(this ISiteTelemetry telemetry, string userId)
    {
        var properties = CreatePropertiesForUser(userId);

        telemetry.TrackEvent("ApiPreferencesSuccess", properties);
    }

    /// <summary>
    /// Tracks an unauthorized API request for preferences.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    public static void TrackApiPreferencesUnauthorized(this ISiteTelemetry telemetry)
    {
        telemetry.TrackEvent("ApiPreferencesUnauthorized");
    }

    /// <summary>
    /// Tracks the claims for a user being updated.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    public static void TrackClaimsUpdated(this ISiteTelemetry telemetry, string userId)
    {
        var properties = CreatePropertiesForUser(userId);

        telemetry.TrackEvent("ClaimsUpdated", properties);
    }

    /// <summary>
    /// Tracks the line preferences for a user being updated.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="oldLines">The user's existing line preferences.</param>
    /// <param name="newLines">The user's new line preferences.</param>
    public static void TrackLinePreferencesUpdated(
        this ISiteTelemetry telemetry,
        string userId,
        ICollection<string> oldLines,
        ICollection<string> newLines)
    {
        var properties = CreatePropertiesForUser(userId);

        properties["OldLines"] = string.Join(',', oldLines ?? []);
        properties["NewLines"] = string.Join(',', newLines ?? []);

        telemetry.TrackEvent("LinePreferencesUpdated", properties);
    }

    /// <summary>
    /// Tracks an unsuccessful request to link an external account.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The external provider.</param>
    public static void TrackLinkExternalAccountFailed(this ISiteTelemetry telemetry, string userId, string provider)
    {
        var properties = CreateProperties(userId, provider);

        telemetry.TrackEvent("LinkExternalAccountFailed", properties);
    }

    /// <summary>
    /// Tracks a successful request to link an external account.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The external provider.</param>
    public static void TrackLinkExternalAccountStart(this ISiteTelemetry telemetry, string? userId, string provider)
    {
        var properties = CreateProperties(userId, provider);

        telemetry.TrackEvent("LinkExternalAccountStart", properties);
    }

    /// <summary>
    /// Tracks a successful request to link an external account.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The external provider.</param>
    public static void TrackLinkExternalAccountSuccess(this ISiteTelemetry telemetry, string userId, string provider)
    {
        var properties = CreateProperties(userId, provider);

        telemetry.TrackEvent("LinkExternalAccountSuccess", properties);
    }

    /// <summary>
    /// Tracks a successful request to remove the Alexa access token.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    public static void TrackRemoveAlexaLink(this ISiteTelemetry telemetry, string userId)
    {
        var properties = CreatePropertiesForUser(userId);

        telemetry.TrackEvent("RemoveAlexaLink", properties);
    }

    /// <summary>
    /// Tracks a successful request to remove a link to an external account.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The external provider.</param>
    public static void TrackRemoveExternalAccountLink(this ISiteTelemetry telemetry, string userId, string provider)
    {
        var properties = CreateProperties(userId, provider);

        telemetry.TrackEvent("RemoveExternalAccountLink", properties);
    }

    /// <summary>
    /// Tracks a user signing in.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The external provider.</param>
    public static void TrackSignIn(this ISiteTelemetry telemetry, string? userId, string provider)
    {
        var properties = CreateProperties(userId, provider);

        telemetry.TrackEvent("SignIn", properties);
    }

    /// <summary>
    /// Tracks a user signing out.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="userId">The user Id.</param>
    public static void TrackSignOut(this ISiteTelemetry telemetry, string? userId)
    {
        var properties = CreatePropertiesForUser(userId);

        telemetry.TrackEvent("SignOut", properties);
    }

    /// <summary>
    /// Tracks a suspicious crawler request.
    /// </summary>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    public static void TrackSuspiciousCrawler(this ISiteTelemetry telemetry)
    {
        telemetry.TrackEvent("SuspiciousCrawler");
    }

    /// <summary>
    /// Creates a properties dictionary container the specified user Id.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> containing the specified user Id.
    /// </returns>
    private static Dictionary<string, string?> CreatePropertiesForUser(string? userId)
    {
        return new Dictionary<string, string?>()
        {
            ["UserId"] = userId,
        };
    }

    /// <summary>
    /// Creates a properties dictionary container the specified user Id and provider.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="provider">The provider.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> containing the specified user Id and provider.
    /// </returns>
    private static Dictionary<string, string?> CreateProperties(string? userId, string provider)
    {
        var properties = CreatePropertiesForUser(userId);

        properties["Provider"] = provider;

        return properties;
    }
}
