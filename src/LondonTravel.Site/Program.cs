// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site;

var builder = WebApplication.CreateBuilder(args);

builder.AddLondonTravelSite();

var app = builder.Build();

app.UseLondonTravelSite();

app.Run();

// Expose the Program class for use with WebApplicationFactory<T>
namespace MartinCostello.LondonTravel.Site
{
    public partial class Program;
}
