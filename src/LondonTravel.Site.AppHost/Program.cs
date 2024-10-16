// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

const string BlobStorage = "AzureBlobStorage";
const string Cosmos = "AzureCosmos";
const string KeyVault = "AzureKeyVault";
const string Storage = "AzureStorage";

var blobs = builder.AddAzureStorage(Storage)
                   .RunAsEmulator()
                   .AddBlobs(BlobStorage);

var cosmos = builder.AddAzureCosmosDB(Cosmos)
                    .RunAsEmulator((container) =>
                    {
                        container.WithDataVolume("londontravel-cosmosdb")
                                 .WithLifetime(ContainerLifetime.Persistent)
                                 .WithPartitionCount(1);
                    })
                    .AddDatabase("LondonTravel");

var secrets = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureKeyVault(KeyVault)
    : builder.AddConnectionString(KeyVault);

builder.AddProject<Projects.LondonTravel_Site>("LondonTravelSite")
       .WithReference(blobs)
       .WithReference(cosmos)
       .WithReference(secrets)
       .WaitFor(blobs)
       .WaitFor(cosmos);

var app = builder.Build();

app.Run();
