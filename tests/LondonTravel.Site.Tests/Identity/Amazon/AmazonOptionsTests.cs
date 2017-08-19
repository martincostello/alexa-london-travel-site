// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Shouldly;
    using Xunit;

    public static class AmazonOptionsTests
    {
        [Fact]
        public static void AmazonOptions_Sets_Properties_Correctly()
        {
            // Act
            var actual = new AmazonOptions();

            // Assert
            actual.CallbackPath.ShouldBe(new PathString("/signin-amazon"));

            actual.AuthorizationEndpoint.ShouldBe(AmazonDefaults.AuthorizationEndpoint);
            actual.TokenEndpoint.ShouldBe(AmazonDefaults.TokenEndpoint);
            actual.UserInformationEndpoint.ShouldBe(AmazonDefaults.UserInformationEndpoint);

            actual.ClaimActions.ShouldNotBeNull();
            actual.ClaimActions.Count().ShouldBeGreaterThanOrEqualTo(4);

            actual.Fields.ShouldNotBeNull();
            actual.Fields.Count.ShouldBe(3);
            actual.Fields.ShouldContain("email");
            actual.Fields.ShouldContain("name");
            actual.Fields.ShouldContain("user_id");

            actual.Scope.ShouldNotBeNull();
            actual.Scope.Count.ShouldBeGreaterThanOrEqualTo(2);
            actual.Scope.ShouldContain("profile");
            actual.Scope.ShouldContain("profile:user_id");
        }
    }
}
