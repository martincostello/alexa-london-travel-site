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
                        // TODO Update persistence configuration when https://github.com/dotnet/aspire/issues/3295 released
                        container.WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1")
                                 .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "true")
                                 .WithVolume("londontravel-cosmosdb", "/tmp/cosmos/appdata");
                    })
                    .AddDatabase("LondonTravel");

var secrets = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureKeyVault(KeyVault)
    : builder.AddConnectionString(KeyVault);

builder.AddProject<Projects.LondonTravel_Site>("LondonTravelSite")
       .WithReference(blobs)
       .WithReference(cosmos)
       .WithReference(secrets);

var app = builder.Build();

app.Run();
