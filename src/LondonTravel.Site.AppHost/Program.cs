// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

// HACK Disabled until there are .NET 9 preview.4 packages available
//var cosmos = builder.AddAzureCosmosDB("Cosmos")
//                    .RunAsEmulator();

builder.AddProject<Projects.LondonTravel_Site>("LondonTravelSite")
       //.WithReference(cosmos)
       ;

var app = builder.Build();

app.Run();
