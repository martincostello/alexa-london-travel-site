// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

const string BlobStorage = "AzureBlobStorage";
const string Cosmos = "AzureCosmos";
const string KeyVault = "AzureKeyVault";
const string Storage = "AzureStorage";

var blobStorage = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureStorage(Storage).AddBlobs(BlobStorage)
    : builder.AddConnectionString(BlobStorage);

var cosmos = builder.AddAzureCosmosDB(Cosmos)
                    .RunAsEmulator()
                    .AddDatabase("LondonTravel");

var secrets = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureKeyVault(KeyVault)
    : builder.AddConnectionString(KeyVault);

builder.AddProject<Projects.LondonTravel_Site>("LondonTravelSite")
       .WithReference(blobStorage)
       .WithReference(cosmos)
       .WithReference(secrets);

var app = builder.Build();

app.Run();
