// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Configures authentication for the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The current application configuration.</param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<LondonTravelUser, LondonTravelRole>((options) => options.User.RequireUniqueEmail = true)
            .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
            .AddRoleStore<RoleStore>()
            .AddUserManager<LondonTravelUserManager>()
            .AddUserStore<UserStore>()
            .AddDefaultTokenProviders();

        services
            .ConfigureApplicationCookie((options) => ConfigureAuthorizationCookie(options, ApplicationCookie.Application.Name))
            .ConfigureExternalCookie((options) => ConfigureAuthorizationCookie(options, ApplicationCookie.External.Name));

        var siteOptions = new SiteOptions();
        configuration.Bind("Site", siteOptions);

        if (siteOptions.Authentication?.IsEnabled == true)
        {
            services
                .AddAuthentication()
                .TryAddAmazon(siteOptions.Authentication)
                .TryAddApple(siteOptions.Authentication)
                .TryAddGitHub(siteOptions.Authentication)
                .TryAddGoogle(siteOptions.Authentication)
                .TryAddMicrosoft(siteOptions.Authentication)
                .TryAddTwitter(siteOptions.Authentication);

            services
                .AddAuthorizationBuilder()
                .AddPolicy("admin", (policy) =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("ADMINISTRATOR");
                });
        }

        return services;
    }

    /// <summary>
    /// Configures an authentication cookie.
    /// </summary>
    /// <param name="options">The cookie authentication options.</param>
    /// <param name="cookieName">The name to use for the cookie.</param>
    private static void ConfigureAuthorizationCookie(CookieAuthenticationOptions options, string cookieName)
    {
        options.AccessDeniedPath = "/account/access-denied/";
        options.Cookie.Name = cookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(150);
        options.LoginPath = "/account/sign-in/";
        options.LogoutPath = "/account/sign-out/";
        options.SlidingExpiration = true;
    }
}
