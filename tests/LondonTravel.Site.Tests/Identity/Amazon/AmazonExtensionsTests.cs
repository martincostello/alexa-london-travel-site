// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public static class AmazonExtensionsTests
    {
        [Fact]
        public static void Can_Add_Amazon_Authentication_To_Service_Collection()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new AuthenticationBuilder(services);

            // Act
            builder.AddAmazon((p) => { });
        }
    }
}
