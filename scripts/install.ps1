#Requires -RunAsAdministrator

param(
    [string]$InstallPath = "C:\Program Files\jeno"
)
$ErrorActionPreference = "Stop"
    
$root = Resolve-Path (Join-Path $PSScriptRoot "..\")

Push-Location $root

$oldPath = $env:Path -split ';' | Where-Object {$_ -like '*jeno*' }

if($oldPath){
    
    Write-Host "Cleaning previous installation"
    if(Test-Path $oldPath){
        Remove-Item $oldPath -Force -Recurse
    }
    
    $env:Path = ($env:Path -split ";" | Where-Object {$_ -ne $oldPath }) -join ";"
}

Write-Output "Publish Jeno file in selected location"
dotnet publish .\src\Jeno\Jeno.csproj --configuration Release --output $InstallPath /p:DebugType=None

Write-Output "Add Jeno location to environment variables"
$env:Path += ";$InstallPath"

Pop-Location