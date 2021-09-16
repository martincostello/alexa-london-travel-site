// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Services.Data;

public static class SystemTextJsonCosmosSerializerTests
{
    [Fact]
    public static async Task Can_Serialize_To_Json_And_Deserialize()
    {
        // Arrange
        var options = new JsonOptions();

        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        options.JsonSerializerOptions.WriteIndented = true;

        var serializer = new SystemTextJsonCosmosSerializer(options.JsonSerializerOptions);

        var input = new LondonTravelUser()
        {
            CreatedAt = DateTimeOffset.UtcNow.UtcDateTime,
            Email = "john.smith@londontravel.martincostello.local",
            ETag = "the-etag",
            Id = "my-id",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // Act
        using var stream = serializer.ToStream(input);

        // Assert
        stream.ShouldNotBeNull();
        stream.Length.ShouldBeGreaterThan(0);

        using var document = await JsonDocument.ParseAsync(stream);
        document.RootElement.GetProperty("createdAt").GetDateTime().ShouldBe(input.CreatedAt);
        document.RootElement.GetProperty("email").GetString().ShouldBe(input.Email);
        document.RootElement.GetProperty("id").GetString().ShouldBe(input.Id);
        document.RootElement.GetProperty("_etag").GetString().ShouldBe(input.ETag);
        document.RootElement.GetProperty("_ts").GetInt64().ShouldBe(input.Timestamp);

        // Arrange
        stream.Seek(0, System.IO.SeekOrigin.Begin);

        // Act
        var actual = serializer.FromStream<LondonTravelUser>(stream);

        // Assert
        actual.ShouldNotBeNull();
        actual.CreatedAt.ShouldBe(input.CreatedAt);
        actual.Email.ShouldBe(input.Email);
        actual.ETag.ShouldBe(input.ETag);
        actual.Id.ShouldBe(input.Id);
        actual.Timestamp.ShouldBe(input.Timestamp);
    }
}
