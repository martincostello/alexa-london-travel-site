$ProgressPreference="SilentlyContinue"

$CertPath = (Join-Path "." ".dockershare")
$Image = "microsoft/azure-documentdb-emulator"

# Switch to Windows daemon
& (Join-Path $env:ProgramFiles "Docker\Docker\dockercli.exe") -SwitchDaemon

# Get the image
docker pull $Image

# Create directory to put the certificate in
mkdir $CertPath -Force | Out-Null
$CertPath = (Resolve-Path $CertPath).Path

# Run the image, mapping its certificate directory to the directory above
docker run --volume "$($CertPath):c:\DocumentDBEmulator\DocumentDBEmulatorCert" --publish-all --tty --interactive --detach $Image | Out-Null

# Import the certificate from the emulator
pushd $CertPath
$CertFile = (Join-Path $CertPath "importcert.ps1")

while ((Test-Path $CertFile) -ne $True) {
    Start-Sleep 2
}

& ".\importcert.ps1"
popd

# Get the container's name
$ContainerName = (docker ps --format "{{.Names}}")

# Get the container IP address
$ContainerIP = (docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' $ContainerName)

# Set the Cosmos DB endpoint
$env:Site:Authentication:UserStore:ServiceUri = "https://$($ContainerIP):8081/"

# Verify the emulator is running
(Invoke-WebRequest "$($env:Site:Authentication:UserStore:ServiceUri)/_explorer/index.html" -UseBasicParsing).StatusCode

# Do stuff
# ...

# Stop the container
docker stop $ContainerName | Out-Null
