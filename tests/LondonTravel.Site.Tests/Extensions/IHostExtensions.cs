// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.LondonTravel.Site.Extensions;

internal static class IHostExtensions
{
    public static Uri GetAddress(this IHost host)
    {
        var server = host.Services.GetRequiredService<IServer>();

        return server.Features.Get<IServerAddressesFeature>() !.Addresses
            .Select((p) => new Uri(p))
            .First();
    }
}
