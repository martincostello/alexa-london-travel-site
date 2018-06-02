// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    /// <summary>
    /// A class representing a cookie used by the application. This class cannot be inherited.
    /// </summary>
    public sealed class ApplicationCookie
    {
        private ApplicationCookie(string name) => Name = name;

        /// <summary>
        /// Gets the anti-forgery cookie.
        /// </summary>
        public static ApplicationCookie Antiforgery => new ApplicationCookie("_anti-forgery");

        /// <summary>
        /// Gets the application identity cookie.
        /// </summary>
        public static ApplicationCookie Application => new ApplicationCookie("london-travel-auth-app");

        /// <summary>
        /// Gets the identity correlation cookie.
        /// </summary>
        public static ApplicationCookie Correlation => new ApplicationCookie("london-travel-auth-correlation");

        /// <summary>
        /// Gets the external identity cookie.
        /// </summary>
        public static ApplicationCookie External => new ApplicationCookie("london-travel-auth-external");

        /// <summary>
        /// Gets the identity state cookie.
        /// </summary>
        public static ApplicationCookie State => new ApplicationCookie("london-travel-auth-state");

        /// <summary>
        /// Gets the name of the cookie.
        /// </summary>
        public string Name { get; }
    }
}
