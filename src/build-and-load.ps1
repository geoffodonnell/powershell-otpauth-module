[CmdletBinding()]
param (
    [Parameter(Position = 0, Mandatory = $false)]
    [string] $Configuration = "Debug",
    [Parameter(Position = 1, Mandatory = $false)]
    [string] $ModuleName = "OtpAuth",
    [Parameter(Position = 2, Mandatory = $false)]
    [string] $Prerelease = "dev"
)

function Get-FullPath {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory = $false)]
        [string] $RelativePath = "Debug"
    )
    $pathSeparator = [System.IO.Path]::DirectorySeparatorChar
    $childPath = $RelativePath -f $pathSeparator

    return [System.IO.Path]::GetFullPath((Join-Path -Path $PSScriptRoot -ChildPath $childPath))
}

$guid = '2035F7AE-BB3D-46E0-8BB9-3C8116B43611'
$projectPath = Get-FullPath -RelativePath ".{0}OtpAuth.PowerShell{0}OtpAuth.PowerShell.csproj"
$buildOutputPath = Get-FullPath -RelativePath ".{0}OtpAuth.PowerShell{0}bin{0}$Configuration{0}_publish_{0}$ModuleName"
$createModuleManifest = Get-FullPath -RelativePath ".{0}create-module-manifest.ps1"

## Clear out the build directory, create if it doesn't exist
if (Test-Path -Path "$buildOutputPath" -ErrorAction SilentlyContinue) {
    Get-ChildItem -Path "$buildOutputPath" -Recurse | Remove-Item -Recurse -Force
} else {
    New-Item -Path "$buildOutputPath" -ItemType Directory | Out-Null
}

## Build
dotnet publish "$projectPath" --configuration "$Configuration" --output "$buildOutputPath" --no-self-contained

## Arrange native assemblies
## SEE: https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/writing-portable-modules?view=powershell-7.4
$runtimesPath = Join-Path -Path $buildOutputPath -ChildPath "runtimes"

Get-ChildItem -Path $runtimesPath | ForEach-Object {

    $target = New-Item -ItemType Directory -Path $buildOutputPath -Name $_.Name

    Get-ChildItem -Path (Join-Path -Path $_.FullName -ChildPath "native") | ForEach-Object {
        Move-Item -Path $_.FullName -Destination $target
    }
}

Remove-Item -Path $runtimesPath -Recurse -Force

## Create the module manifest
Invoke-Expression "$createModuleManifest -Path '$buildOutputPath' -Guid $guid -Prerelease '$Prerelease'"

## Import the module
$modulePath = Join-Path -Path $buildOutputPath -ChildPath "$ModuleName.psd1"

Import-Module -Name $modulePath