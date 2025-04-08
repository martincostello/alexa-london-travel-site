#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][string] $Filter = "",
    [Parameter(Mandatory = $false)][string] $Job = ""
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

$installDotNetSdk = $false

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {
    ${env:DOTNET_INSTALL_DIR} = Join-Path $solutionPath ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath
    }

    ${env:PATH} = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
    $dotnet = Join-Path ${env:DOTNET_INSTALL_DIR} "dotnet"
}
else {
    $dotnet = "dotnet"
}

$benchmarks = (Join-Path $solutionPath "perf" "LondonTravel.Site.Benchmarks" "LondonTravel.Site.Benchmarks.csproj")

Write-Information "Running benchmarks..."

$additionalArgs = @()

if (-Not [string]::IsNullOrEmpty($Filter)) {
    $additionalArgs += "--filter"
    $additionalArgs += $Filter
}

if (-Not [string]::IsNullOrEmpty($Job)) {
    $additionalArgs += "--job"
    $additionalArgs += $Job
}

if (-Not [string]::IsNullOrEmpty(${env:GITHUB_SHA})) {
    $additionalArgs += "--exporters"
    $additionalArgs += "json"
}

& $dotnet run --project $benchmarks --configuration "Release" -- $additionalArgs
