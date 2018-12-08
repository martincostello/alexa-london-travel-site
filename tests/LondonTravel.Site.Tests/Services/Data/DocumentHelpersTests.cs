// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Net.Http;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.Azure.Documents;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Shouldly;
    using Xunit;

    public static class DocumentHelpersTests
    {
        [Fact]
        public static void CreateClient_Throws_If_ServiceProvider_Is_Null()
        {
            // Arrange
            IServiceProvider serviceProvider = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("serviceProvider", () => DocumentHelpers.CreateClient(serviceProvider));
        }

        [Fact]
        public static void CreateClient_Throws_If_Options_Is_Null()
        {
            // Arrange
            UserStoreOptions options = null;
            HttpMessageHandler handler = Mock.Of<HttpMessageHandler>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Fact]
        public static void CreateClient_Throws_If_Handler_Is_Null()
        {
            // Arrange
            UserStoreOptions options = new UserStoreOptions();
            HttpMessageHandler handler = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("handler", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Fact]
        public static void CreateClient_Throws_If_Options_Has_No_Service_Endpoint()
        {
            // Arrange
            var handler = Mock.Of<HttpMessageHandler>();

            var options = new UserStoreOptions()
            {
                ServiceUri = null,
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = "my-database",
                CollectionName = "my-collection",
            };

            // Act and Assert
            Assert.Throws<ArgumentException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Fact]
        public static void CreateClient_Throws_If_Options_Has_Relative_Service_Endpoint()
        {
            // Arrange
            var handler = Mock.Of<HttpMessageHandler>();

            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("/", UriKind.Relative),
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = "my-database",
                CollectionName = "my-collection",
            };

            // Act and Assert
            Assert.Throws<ArgumentException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public static void CreateClient_Throws_If_Options_Has_No_Access_Key(string accessKey)
        {
            // Arrange
            var handler = Mock.Of<HttpMessageHandler>();

            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("https://cosmosdb.azure.local"),
                AccessKey = accessKey,
                DatabaseName = "my-database",
                CollectionName = "my-collection",
            };

            // Act and Assert
            Assert.Throws<ArgumentException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public static void CreateClient_Throws_If_Options_Has_No_Database_Name(string databaseName)
        {
            // Arrange
            var handler = Mock.Of<HttpMessageHandler>();

            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("https://cosmosdb.azure.local"),
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = databaseName,
                CollectionName = "my-collection",
            };

            // Act and Assert
            Assert.Throws<ArgumentException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public static void CreateClient_Throws_If_Options_Has_No_Collection_Name(string collectionName)
        {
            // Arrange
            var handler = Mock.Of<HttpMessageHandler>();

            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("https://cosmosdb.azure.local"),
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = "my-database",
                CollectionName = collectionName,
            };

            // Act and Assert
            Assert.Throws<ArgumentException>("options", () => DocumentHelpers.CreateClient(options, handler));
        }

        [Fact]
        public static void CreateClient_Creates_Client_From_Service_Provider_With_Locations()
        {
            // Arrange
            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("https://cosmosdb.azure.local"),
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = "my-database",
                CollectionName = "my-collection",
                CurrentLocation = "UK South",
                PreferredLocations = new[] { "UK South", "East US" },
            };

            var services = new ServiceCollection()
                .AddSingleton(options)
                .AddHttpClient();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            IDocumentClient actual = DocumentHelpers.CreateClient(serviceProvider);

            // Assert
            actual.ShouldNotBeNull();
        }

        [Fact]
        public static void CreateClient_Creates_Client_From_Service_Provider_With_No_Locations()
        {
            // Arrange
            var options = new UserStoreOptions()
            {
                ServiceUri = new Uri("https://cosmosdb.azure.local"),
                AccessKey = "bpfYUKmfV0arChaIPI3hU3+bn3w=",
                DatabaseName = "my-database",
                CollectionName = "my-collection",
            };

            var services = new ServiceCollection()
                .AddSingleton(options)
                .AddHttpClient();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            IDocumentClient actual = DocumentHelpers.CreateClient(serviceProvider);

            // Assert
            actual.ShouldNotBeNull();
        }
    }
}
