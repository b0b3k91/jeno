#Requires -RunAsAdministrator

param(
    [string]$InstallPath = "C:\Program Files\jeno"
)
$ErrorActionPreference = "Stop"
    
$root = Resolve-Path (Join-Path $PSScriptRoot "..\")
Push-Location $root

$path = [System.Environment]::GetEnvironmentVariable('PATH', 'Machine')
$jenoPaths = @($path -split ';' | Where-Object { $_ -like '*jeno*' })
if ($jenoPaths.Count -gt 0) {
    
    Write-Host "Cleaning previous installation"
    foreach ($jenoPath in $jenoPaths) {
        if (Test-Path $jenoPath) {
            Remove-Item $jenoPath -Force -Recurse
        }
        
        $newPath = ($Env:Path -split ";" | Where-Object { $_ -ne $jenoPath }) -join ";"
        [Environment]::SetEnvironmentVariable("Path", $newPath, [System.EnvironmentVariableTarget]::Machine)
    }
}

Write-Output "Publish Jeno in selected location"
dotnet publish .\src\Jeno\Jeno.csproj --configuration Release --output $InstallPath /p:DebugType=None

Write-Output "Get permission to saving and modifying Jeno configuration file"

$configurationFile = Join-Path $InstallPath "appsettings.json"
$acl = Get-Acl -Path $configurationFile

$acl | Select-Object -ExpandProperty Access | Select-Object -ExpandProperty IdentityReference | ForEach-Object {
    $identity = $_
    try{
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity,"FullControl","Allow")
        $acl.SetAccessRule($accessRule)
    }
    catch [System.Security.Principal.IdentityNotMappedException] {
        Write-Warning "Cannot allow `"$($identity.Value)`" identity to modifying appsettings.json file."
    }
}

$acl | Set-Acl $configurationFile

Write-Output "Add Jeno to environment variables"

$pathWithJeno = ([System.Environment]::GetEnvironmentVariable('PATH', 'Machine')) + ";$InstallPath"
[Environment]::SetEnvironmentVariable("Path", $pathWithJeno, [System.EnvironmentVariableTarget]::Machine)

Pop-Location