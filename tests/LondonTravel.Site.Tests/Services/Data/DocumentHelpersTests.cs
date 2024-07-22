// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Option = Microsoft.Extensions.Options.Options;

namespace MartinCostello.LondonTravel.Site.Services.Data;

public static class DocumentHelpersTests
{
    [Fact]
    public static void CreateClient_Throws_If_ServiceProvider_Is_Null()
    {
        // Arrange
        IServiceProvider? serviceProvider = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("serviceProvider", () => DocumentHelpers.CreateClient(serviceProvider!));
    }

    [Fact]
    public static void CreateClient_Throws_If_ConnectionString_Is_Null()
    {
        // Arrange
        string connectionString = null!;
        var options = new UserStoreOptions();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(
            "connectionString",
            () => DocumentHelpers.CreateClient(connectionString, options, httpClientFactory!));
    }

    [Fact]
    public static void CreateClient_Throws_If_HttpClientFactory_Is_Null()
    {
        // Arrange
        string connectionString = "AccessToken=bpfYUKmfV0arChaIPI3hU3+bn3w=;Endpoint=https://cosmosdb.azure.local";
        var options = new UserStoreOptions();
        IHttpClientFactory? httpClientFactory = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(
            "httpClientFactory",
            () => DocumentHelpers.CreateClient(connectionString, options, httpClientFactory!));
    }

    [Fact]
    public static void CreateClient_Throws_If_Options_Is_Null()
    {
        // Arrange
        string connectionString = "AccessToken=bpfYUKmfV0arChaIPI3hU3+bn3w=;Endpoint=https://cosmosdb.azure.local";
        UserStoreOptions? options = null;
        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(
            "options",
            () => DocumentHelpers.CreateClient(connectionString, options!, httpClientFactory));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public static void CreateClient_Throws_If_Options_Has_No_Database_Name(string? databaseName)
    {
        // Arrange
        string connectionString = "AccountKey=bpfYUKmfV0arChaIPI3hU3+bn3w=;AccountEndpoint=https://cosmosdb.azure.local";
        var options = new UserStoreOptions()
        {
            DatabaseName = databaseName,
            CollectionName = "my-collection",
        };

        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        // Act and Assert
        Assert.Throws<ArgumentException>(
            "options",
            () => DocumentHelpers.CreateClient(connectionString, options, httpClientFactory));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public static void CreateClient_Throws_If_Options_Has_No_Collection_Name(string? collectionName)
    {
        // Arrange
        string connectionString = "AccountKey=bpfYUKmfV0arChaIPI3hU3+bn3w=;AccountEndpoint=https://cosmosdb.azure.local";
        var options = new UserStoreOptions()
        {
            DatabaseName = "my-database",
            CollectionName = collectionName,
        };

        var httpClientFactory = Substitute.For<IHttpClientFactory>();

        // Act and Assert
        Assert.Throws<ArgumentException>(
            "options",
            () => DocumentHelpers.CreateClient(connectionString, options, httpClientFactory));
    }

    [Fact]
    public static void CreateClient_Creates_Client_From_Service_Provider_With_Locations()
    {
        // Arrange
        string connectionString = "AccountKey=bpfYUKmfV0arChaIPI3hU3+bn3w=;AccountEndpoint=https://cosmosdb.azure.local";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([KeyValuePair.Create<string, string?>("ConnectionStrings:AzureCosmos", connectionString)])
            .Build();

        var options = new UserStoreOptions()
        {
            DatabaseName = "my-database",
            CollectionName = "my-collection",
            CurrentLocation = "UK South",
        };

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddHttpClient()
            .AddSingleton(Option.Create(new JsonOptions()))
            .AddSingleton(options);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        using var actual = DocumentHelpers.CreateClient(serviceProvider);

        // Assert
        actual.ShouldNotBeNull();
    }

    [Fact]
    public static void CreateClient_Creates_Client_From_Service_Provider_With_No_Locations()
    {
        // Arrange
        string connectionString = "AccountKey=bpfYUKmfV0arChaIPI3hU3+bn3w=;AccountEndpoint=https://cosmosdb.azure.local";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([KeyValuePair.Create<string, string?>("ConnectionStrings:AzureCosmos", connectionString)])
            .Build();

        var options = new UserStoreOptions()
        {
            DatabaseName = "my-database",
            CollectionName = "my-collection",
        };

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddHttpClient()
            .AddSingleton(Option.Create(new JsonOptions()))
            .AddSingleton(options);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        using var actual = DocumentHelpers.CreateClient(serviceProvider);

        // Assert
        actual.ShouldNotBeNull();
    }
}
