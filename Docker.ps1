$ErrorActionPreference = "Stop"
$ProgressPreference="SilentlyContinue"

$RepoPath = Split-Path $MyInvocation.MyCommand.Definition
$CertPath = (Join-Path $RepoPath ".dockershare")
$Image = "microsoft/azure-documentdb-emulator"

# Get the image
Write-Host "Downloading $($Image) docker image for Azure Cosmos DB emulator..." -ForegroundColor Green
docker pull $Image

# Create directory to put the certificate in
mkdir $CertPath -Force | Out-Null

# Run the image, mapping its certificate directory to the directory above
Write-Host "Starting Azure Cosmos DB emulator..." -ForegroundColor Green
docker run --volume "$($CertPath):c:\DocumentDBEmulator\DocumentDBEmulatorCert" --publish-all --tty --interactive --detach $Image | Out-Null

# Import the certificate from the emulator
pushd $CertPath
$CertFile = (Join-Path $CertPath "importcert.ps1")

Write-Host "Waiting for Azure Cosmos DB emulator to export TLS certificate..." -ForegroundColor Green
while ((Test-Path $CertFile) -ne $True) {
    Start-Sleep 2
}

Write-Host "Installing Azure Cosmos DB emulator TLS certificate..." -ForegroundColor Green
& ".\importcert.ps1"
popd

# Get the container's name
$ContainerName = (docker ps --format "{{.Names}}")

# Get the container IP address
$ContainerIP = (docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' $ContainerName)

# Set the Cosmos DB endpoint
$ServiceUri = "https://$($ContainerIP):8081/"
[Environment]::SetEnvironmentVariable("Site:Authentication:UserStore:ServiceUri", $ServiceUri, [System.EnvironmentVariableTarget]::Machine)

# Verify the emulator is running
if ((Invoke-WebRequest "$($ServiceUri)_explorer/index.html" -UseBasicParsing).StatusCode -ne 200) {
    Write-Host "Failed to verify Azure Cosmos DB emulator at $($ServiceUri)." -ForegroundColor Red
} else {
    Write-Host "Azure Cosmos DB emulator is listening on $($ServiceUri)."
}
