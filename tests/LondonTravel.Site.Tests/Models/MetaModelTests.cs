// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using MartinCostello.LondonTravel.Site.Options;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// A class containing unit tests for the <see cref="MetaModel"/> class.
    /// </summary>
    public static class MetaModelTests
    {
        [Fact]
        public static void MetaModel_Create_Handles_Null_Options()
        {
            // Arrange
            var options = null as MetadataOptions;

            // Act
            MetaModel actual = MetaModel.Create(options);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public static void MetaModel_Create_Handles_Null_Author()
        {
            // Arrange
            var options = new MetadataOptions();

            // Act
            MetaModel actual = MetaModel.Create(options);

            // Assert
            actual.ShouldNotBeNull();
        }
    }
}
