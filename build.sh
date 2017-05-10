#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts
export configuration=Release

dotnet restore LondonTravel.Site.sln --verbosity minimal || exit 1
dotnet publish src/LondonTravel.Site/LondonTravel.Site.csproj --output $artifacts --configuration $configuration || exit 1
dotnet test tests/LondonTravel.Site.Tests/LondonTravel.Site.Tests.csproj --output $artifacts --configuration $configuration || exit 1
