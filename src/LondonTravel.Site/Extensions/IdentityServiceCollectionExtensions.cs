// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using MartinCostello.LondonTravel.Site.Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IdentityServiceCollectionExtensions
    {
        /// <summary>
        /// Configures authentication for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services)
        {
            services
                .AddIdentity<LondonTravelUser, LondonTravelRole>((options) => options.User.RequireUniqueEmail = true)
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>()
                .AddRoleStore<RoleStore>()
                .AddUserStore<UserStore>()
                .AddDefaultTokenProviders();

            services
                .ConfigureApplicationCookie((options) => ConfigureAuthorizationCookie(options, ApplicationCookie.Application.Name))
                .ConfigureExternalCookie((options) => ConfigureAuthorizationCookie(options, ApplicationCookie.External.Name));

            var provider = services.BuildServiceProvider();
            var siteOptions = provider.GetRequiredService<SiteOptions>();

            if (siteOptions?.Authentication?.IsEnabled == true)
            {
                var builder = services
                    .AddAuthentication()
                    .AsApplicationBuilder(provider);

                builder
                    .TryAddAmazon()
                    .TryAddFacebook()
                    .TryAddGoogle()
                    .TryAddMicrosoft()
                    .TryAddTwitter();
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

        /// <summary>
        /// Returns an <see cref="ApplicationAuthorizationBuilder"/> used to configure authentication for the application.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to create the builder with.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The <see cref="ApplicationAuthorizationBuilder"/> to use to configure authentication for the application.
        /// </returns>
        private static ApplicationAuthorizationBuilder AsApplicationBuilder(this AuthenticationBuilder builder, IServiceProvider serviceProvider)
        {
            return new ApplicationAuthorizationBuilder(builder, serviceProvider);
        }
    }
}
