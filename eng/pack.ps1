[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputRoot,
    [string]$VersionSuffix,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
}

function Get-VersionInfo {
    param(
        [string]$RepoRoot,
        [string]$OverrideSuffix
    )

    [xml]$versions = Get-Content -Raw (Join-Path $RepoRoot "eng\Versions.props")
    $baseVersion = $versions.Project.PropertyGroup.EasyUseVersion
    $configuredSuffix = $versions.Project.PropertyGroup.EasyUsePrereleaseSuffix
    $resolvedSuffix = if ($PSBoundParameters.ContainsKey("OverrideSuffix") -and $null -ne $OverrideSuffix -and $OverrideSuffix -ne "") {
        if ($OverrideSuffix.StartsWith("-")) { $OverrideSuffix } else { "-$OverrideSuffix" }
    }
    else {
        $configuredSuffix
    }

    return [pscustomobject]@{
        BaseVersion = $baseVersion
        Suffix = $resolvedSuffix
        FullVersion = "$baseVersion$resolvedSuffix"
    }
}

$repoRoot = Get-RepoRoot
$version = Get-VersionInfo -RepoRoot $repoRoot -OverrideSuffix $VersionSuffix

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\packages\$($version.FullVersion)"
}

$projects = @(
    "src\AppAutomation.Abstractions\AppAutomation.Abstractions.csproj",
    "src\AppAutomation.Authoring\AppAutomation.Authoring.csproj",
    "src\AppAutomation.Session.Contracts\AppAutomation.Session.Contracts.csproj",
    "src\AppAutomation.TUnit\AppAutomation.TUnit.csproj",
    "src\AppAutomation.FlaUI\AppAutomation.FlaUI.csproj",
    "src\AppAutomation.Avalonia.Headless\AppAutomation.Avalonia.Headless.csproj"
)

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

foreach ($project in $projects) {
    $projectPath = Join-Path $repoRoot $project
    $arguments = @(
        "pack",
        $projectPath,
        "-c", $Configuration,
        "--output", $OutputRoot,
        "/p:EasyUsePrereleaseSuffix=$($version.Suffix)"
    )

    if ($NoBuild) {
        $arguments += "--no-build"
    }

    Write-Host "Packing $project -> $OutputRoot"
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for $project"
    }
}

Write-Host "Packed AppAutomation version $($version.FullVersion) into $OutputRoot"
