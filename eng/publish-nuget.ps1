[CmdletBinding()]
param(
    [string]$PackagesPath,
    [string]$Source = $env:NUGET_SOURCE,
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$SymbolSource = $env:NUGET_SYMBOL_SOURCE,
    [string]$SymbolApiKey = $env:NUGET_SYMBOL_API_KEY,
    [switch]$SkipDuplicate = $true
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
}

function Get-FullVersion {
    param([string]$RepoRoot)

    [xml]$versions = Get-Content -Raw (Join-Path $RepoRoot "eng\Versions.props")
    return "$($versions.Project.PropertyGroup.EasyUseVersion)$($versions.Project.PropertyGroup.EasyUsePrereleaseSuffix)"
}

$repoRoot = Get-RepoRoot

if ([string]::IsNullOrWhiteSpace($PackagesPath)) {
    $PackagesPath = Join-Path $repoRoot "artifacts\packages\$(Get-FullVersion -RepoRoot $repoRoot)"
}

if ([string]::IsNullOrWhiteSpace($Source)) {
    throw "NuGet source is required. Pass -Source or set NUGET_SOURCE."
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API key is required. Pass -ApiKey or set NUGET_API_KEY."
}

if (-not (Test-Path $PackagesPath)) {
    throw "Packages path was not found: $PackagesPath"
}

$packageFiles = Get-ChildItem -Path $PackagesPath -Filter "*.nupkg" |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object Name

if ($packageFiles.Count -eq 0) {
    throw "No .nupkg files were found in $PackagesPath"
}

foreach ($package in $packageFiles) {
    $arguments = @(
        "nuget", "push", $package.FullName,
        "--source", $Source,
        "--api-key", $ApiKey
    )

    if ($SkipDuplicate) {
        $arguments += "--skip-duplicate"
    }

    Write-Host "Publishing $($package.Name) -> $Source"
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for $($package.FullName)"
    }
}

$symbolPackages = Get-ChildItem -Path $PackagesPath -Filter "*.snupkg" | Sort-Object Name
if (-not [string]::IsNullOrWhiteSpace($SymbolSource) -and $symbolPackages.Count -gt 0) {
    $effectiveSymbolApiKey = if ([string]::IsNullOrWhiteSpace($SymbolApiKey)) { $ApiKey } else { $SymbolApiKey }

    foreach ($package in $symbolPackages) {
        $arguments = @(
            "nuget", "push", $package.FullName,
            "--source", $SymbolSource,
            "--api-key", $effectiveSymbolApiKey
        )

        if ($SkipDuplicate) {
            $arguments += "--skip-duplicate"
        }

        Write-Host "Publishing symbols $($package.Name) -> $SymbolSource"
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet nuget push failed for symbol package $($package.FullName)"
        }
    }
}
elseif ($symbolPackages.Count -gt 0) {
    Write-Host "Symbol packages were found but SymbolSource was not provided. Skipping .snupkg publish."
}
