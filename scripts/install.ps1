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

    $newPath = ($Env:Path -split ";" | Where-Object {$_ -ne $oldPath }) -join ";"
    [Environment]::SetEnvironmentVariable("Path", $newPath, [System.EnvironmentVariableTarget]::Machine)
}

Write-Output "Publish Jeno in selected location"
dotnet publish .\src\Jeno\Jeno.csproj --configuration Release --output $InstallPath /p:DebugType=None

Write-Output "Add Jeno to environment variables"
[Environment]::SetEnvironmentVariable("Path", ($Env:Path += ";$InstallPath"), [System.EnvironmentVariableTarget]::Machine)

Pop-Location