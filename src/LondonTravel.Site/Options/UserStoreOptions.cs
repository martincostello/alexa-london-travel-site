// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    using System;

    /// <summary>
    /// A class representing the authentication user store options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class UserStoreOptions
    {
        /// <summary>
        /// Gets or sets the Azure DocumentDb service URI to use.
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// Gets or sets the Azure DocumentDb access key to use.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the Azure DocumentDb database to use.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Azure DocumentDb collection to use.
        /// </summary>
        public string CollectionName { get; set; }
    }
}
