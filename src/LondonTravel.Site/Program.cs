// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Extensions;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class representing the entry-point to the application. This class cannot be inherited.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry-point to the application.
    /// </summary>
    /// <param name="args">The arguments to the application.</param>
    /// <returns>
    /// The exit code from the application.
    /// </returns>
    public static int Main(string[] args)
    {
        try
        {
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Console.Error.WriteLine($"Unhandled exception: {ex}");
            return -1;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureApplication()
            .ConfigureWebHostDefaults(
                (webBuilder) =>
                {
                    webBuilder.CaptureStartupErrors(true)
                              .ConfigureKestrel((p) => p.AddServerHeader = false)
                              .UseStartup<Startup>();
                });
    }
}
