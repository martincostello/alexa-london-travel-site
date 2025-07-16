// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace MartinCostello.LondonTravel.Site.Extensions;

internal static class IServerExtensions
{
    public static Uri? GetAddress(this IServer server)
    {
        return server.Features.Get<IServerAddressesFeature>()!.Addresses
            .Select((p) => new Uri(p))
            .FirstOrDefault();
    }
}
