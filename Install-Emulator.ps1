$ErrorActionPreference = "Stop"
$ProgressPreference="SilentlyContinue"

Write-Host "Setting up Azure Cosmos DB emulator..." -ForegroundColor Green

$RepoPath = Split-Path $MyInvocation.MyCommand.Definition
$CertPath = (Join-Path $RepoPath ".dockershare")
$Image = "microsoft/azure-documentdb-emulator"

# Get the image
Write-Host "Downloading $($Image) docker image for Azure Cosmos DB emulator..."
$job = Start-Job -ScriptBlock { docker pull "microsoft/azure-documentdb-emulator" }
$job | Wait-Job -Timeout 600 | Out-Null
if ($job.State -eq "Running") {
  $job.StopJob()
  throw "Failed to download $($Image) docker image within 10 minutes."
}

# Create directory to put the certificate in
mkdir $CertPath -Force | Out-Null

# Run the image, mapping its certificate directory to the directory above
Write-Host "Starting the Azure Cosmos DB emulator..."
docker run --volume "$($CertPath):c:\DocumentDBEmulator\DocumentDBEmulatorCert" --publish-all --tty --interactive --detach $Image | Out-Null

# Import the certificate from the emulator
$CertFile = (Join-Path $CertPath "importcert.ps1")

Write-Host "Waiting for Azure Cosmos DB emulator to export TLS certificate..."

$job = Start-Job -ScriptBlock { while ((Test-Path $args[0]) -ne $True) { Start-Sleep 2 } } -ArgumentList $CertFile
$job | Wait-Job -Timeout 60 | Out-Null
if ($job.State -eq "Running") {
  $job.StopJob()
  throw "The Azure Cosmos DB emulator did not export the TLS certificate within 60 seconds."
}

pushd $CertPath
Write-Host "Installing Azure Cosmos DB emulator TLS certificate..."
& ".\importcert.ps1"
popd

# Get the container's name
$ContainerName = (docker ps --format "{{.Names}}")

# Get the container IP address
$ContainerIP = (docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' $ContainerName)

# Set the Cosmos DB endpoint
$ServiceUri = "https://$($ContainerIP):8081/"

# Verify the emulator is running
if ((Invoke-WebRequest "$($ServiceUri)_explorer/index.html" -UseBasicParsing).StatusCode -ne 200) {
    Write-Host "Failed to verify Azure Cosmos DB emulator at $($ServiceUri)." -ForegroundColor Red
} else {
    Write-Host "Azure Cosmos DB emulator is listening on $($ServiceUri)." -ForegroundColor Green
}

return $ServiceUri
