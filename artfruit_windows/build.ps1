<#
    build.ps1 — builds a self-contained, single-file ArtFruit.exe for Windows.

    Usage (from a Windows PowerShell / pwsh prompt, or via WSL interop):
        pwsh ./build.ps1              # Release publish -> bin/publish/ArtFruit.exe
        pwsh ./build.ps1 -Run         # ...then launch it
        pwsh ./build.ps1 -Configuration Debug

    Requires the .NET 8 SDK on the Windows host (dotnet --version >= 8).
#>
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$Run
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$project = "ArtFruit.Windows.csproj"
$publishDir = Join-Path $scriptDir "bin\publish"

Write-Host "==> Publishing ArtFruit ($Configuration, $Runtime)..." -ForegroundColor Cyan

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

$exe = Join-Path $publishDir "ArtFruit.exe"
if (Test-Path $exe) {
    Write-Host ""
    Write-Host "OK  Built: $exe" -ForegroundColor Green
    if ($Run) {
        Write-Host "==> Launching ArtFruit..." -ForegroundColor Cyan
        Start-Process $exe
    }
} else {
    Write-Error "Build finished but ArtFruit.exe was not found in $publishDir"
    exit 1
}
