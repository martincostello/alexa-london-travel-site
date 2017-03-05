// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    using System;

    /// <summary>
    /// A class representing the options for reports. This class cannot be inherited.
    /// </summary>
    public sealed class ReportOptions
    {
        /// <summary>
        /// Gets or sets the URI to use for <c>Content-Security-Policy</c>.
        /// </summary>
        public Uri ContentSecurityPolicy { get; set; }

        /// <summary>
        /// Gets or sets the URI to use for <c>Content-Security-Policy-Report-Only</c>.
        /// </summary>
        public Uri ContentSecurityPolicyReportOnly { get; set; }

        /// <summary>
        /// Gets or sets the URI to use for <c>Public-Key-Pins</c>.
        /// </summary>
        public Uri PublicKeyPins { get; set; }

        /// <summary>
        /// Gets or sets the URI to use for <c>Public-Key-Pins-Report-Only</c>.
        /// </summary>
        public Uri PublicKeyPinsReportOnly { get; set; }
    }
}
