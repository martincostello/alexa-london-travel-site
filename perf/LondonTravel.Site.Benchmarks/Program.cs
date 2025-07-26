// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.LondonTravel.Site.Benchmarks;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmark = new AppBenchmarks();
    await benchmark.StartServer();

    try
    {
        _ = await benchmark.Root();
        _ = await benchmark.Version();
    }
    finally
    {
        await benchmark.StopServer();
    }

    return 0;
}
else
{
    var summary = BenchmarkRunner.Run<AppBenchmarks>(args: args);
    return summary.Reports.Any((p) => !p.Success) ? 1 : 0;
}
