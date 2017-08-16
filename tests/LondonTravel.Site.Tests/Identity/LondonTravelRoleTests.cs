// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Security.Claims;
    using Shouldly;
    using Xunit;

    public static class LondonTravelRoleTests
    {
        [Fact]
        public static void FromClaim_Throws_If_Claim_Is_Null()
        {
            // Arrange
            Claim claim = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LondonTravelRole.FromClaim(claim));
        }

        [Fact]
        public static void LondonTravelRole_Can_Convert_From_And_To_Claim()
        {
            // Arrange
            Claim expected = new Claim("type", "value", "valueType", "issuer");

            // Act and Assert
            var role = LondonTravelRole.FromClaim(expected);

            // Assert
            role.ShouldNotBeNull();
            role.ClaimType.ShouldBe(expected.Type);
            role.Issuer.ShouldBe(expected.Issuer);
            role.Value.ShouldBe(expected.Value);
            role.ValueType.ShouldBe(expected.ValueType);

            // Act
            var actual = role.ToClaim();

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeSameAs(expected);
            actual.Issuer.ShouldBe(expected.Issuer);
            actual.Type.ShouldBe(expected.Type);
            actual.Value.ShouldBe(expected.Value);
            actual.ValueType.ShouldBe(expected.ValueType);
        }
    }
}
