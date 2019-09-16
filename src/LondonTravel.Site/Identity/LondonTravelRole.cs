// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Security.Claims;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A class representing a user role.
    /// </summary>
    public class LondonTravelRole
    {
        /// <summary>
        /// Gets or sets the role Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the claim's type.
        /// </summary>
        [JsonPropertyName("claimType")]
        public string? ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim's value.
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the claim's value's type.
        /// </summary>
        [JsonPropertyName("valueType")]
        public string? ValueType { get; set; }

        /// <summary>
        /// Gets or sets the claim's issuer.
        /// </summary>
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="LondonTravelRole"/> from an instance of <see cref="Claim"/>.
        /// </summary>
        /// <param name="claim">The claim to create the instance from.</param>
        /// <returns>
        /// An instance of <see cref="LondonTravelRole"/> created from <paramref name="claim"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="claim"/> is <see langword="null"/>.
        /// </exception>
        public static LondonTravelRole FromClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            return new LondonTravelRole()
            {
                ClaimType = claim.Type,
                Issuer = claim.Issuer,
                Value = claim.Value,
                ValueType = claim.ValueType,
            };
        }

        /// <summary>
        /// Creates an instance of <see cref="Claim"/> from the current instance.
        /// </summary>
        /// <returns>
        /// The created instance of <see cref="Claim"/>.
        /// </returns>
        public Claim ToClaim()
        {
            return new Claim(
                ClaimType ?? string.Empty,
                Value ?? string.Empty,
                ValueType ?? ClaimValueTypes.String,
                Issuer ?? "LondonTravel");
        }
    }
}
