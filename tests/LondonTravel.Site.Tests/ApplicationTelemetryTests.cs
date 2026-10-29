// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Middleware;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class containing unit tests for the <see cref="ApplicationTelemetry"/> class.
/// </summary>
public static class ApplicationTelemetryTests
{
    [Fact]
    public static async Task ProfileAsync_Invokes_Operation_If_There_Is_No_Baggage()
    {
        // Arrange
        Baggage.Current = default;

        var operation = new TaskCompletionSource();
        string? actual = null;

        // Act
        var task = ApplicationTelemetry.ProfileAsync(
            "state",
            (state) =>
            {
                actual = state;
                return operation.Task;
            });

        // Assert
        actual.ShouldBe("state");
        task.ShouldBeSameAs(operation.Task);

        operation.SetResult();
        await task;
    }

    [Fact]
    public static async Task ProfileAsync_Invokes_Operation_If_There_Is_No_K6_Baggage()
    {
        // Arrange
        var invocations = 0;

        try
        {
            Baggage.SetBaggage("other", "value");
            Baggage.SetBaggage("k6", "value");

            // Act
            await ApplicationTelemetry.ProfileAsync(
                "state",
                (state) =>
                {
                    invocations++;
                    return Task.CompletedTask;
                });

            // Assert
            invocations.ShouldBe(1);
        }
        finally
        {
            Baggage.Current = default;
        }
    }

    [Fact]
    public static async Task ProfileAsync_Invokes_Operation_If_There_Is_K6_Baggage()
    {
        // Arrange
        var invocations = 0;

        try
        {
            Baggage.SetBaggage("k6.test_run_id", "42");
            Baggage.SetBaggage("k6.empty", string.Empty);
            Baggage.SetBaggage("other", "value");

            // Act
            await ApplicationTelemetry.ProfileAsync(
                "state",
                async (state) =>
                {
                    await Task.Yield();
                    invocations++;
                });

            // Assert
            invocations.ShouldBe(1);
        }
        finally
        {
            Baggage.Current = default;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task ProfileAsync_Propagates_Exceptions_From_Operation(bool hasK6Baggage)
    {
        // Arrange
        try
        {
            Baggage.Current = default;

            if (hasK6Baggage)
            {
                Baggage.SetBaggage("k6.test_run_id", "42");
            }

            // Act and Assert
            await Should.ThrowAsync<InvalidOperationException>(
                () => ApplicationTelemetry.ProfileAsync<object?>(
                    null,
                    (_) => Task.FromException(new InvalidOperationException("Test"))));
        }
        finally
        {
            Baggage.Current = default;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task PyroscopeK6Middleware_Invokes_Next_Middleware(bool hasK6Baggage)
    {
        // Arrange
        var invocations = 0;
        var context = new DefaultHttpContext();

        var target = new PyroscopeK6Middleware(
            (_) =>
            {
                invocations++;
                return Task.CompletedTask;
            });

        try
        {
            Baggage.Current = default;

            if (hasK6Baggage)
            {
                Baggage.SetBaggage("k6.test_run_id", "42");
            }

            // Act
            await target.InvokeAsync(context);

            // Assert
            invocations.ShouldBe(1);
        }
        finally
        {
            Baggage.Current = default;
        }
    }
}
