// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site;

internal static class TaskExtensions
{
    public static async Task<T2> ThenAsync<T1, T2>(this Task<T1> value, Func<T1, Task<T2>> continuation)
    {
        return await continuation(await value);
    }

    public static async Task ShouldBe(this Task<string> task, string expected)
    {
        string actual = await task;
        actual.ShouldBe(expected);
    }

    public static async Task ShouldBe<T>(this Task<T> task, T expected)
    {
        var actual = await task;
        actual.ShouldBe(expected);
    }

    public static async Task ShouldBeFalse(this Task<bool> task)
    {
        bool actual = await task;
        actual.ShouldBeFalse();
    }

    public static async Task ShouldBeTrue(this Task<bool> task)
    {
        bool actual = await task;
        actual.ShouldBeTrue();
    }

    public static async Task ShouldNotBeNullOrWhiteSpace(this Task<string> task)
    {
        string actual = await task;
        actual.ShouldNotBeNullOrWhiteSpace();
    }
}
