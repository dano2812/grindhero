param(
    [string]$ProjectPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'game')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$required = @(
    'Assets', 'Packages/manifest.json', 'Packages/packages-lock.json',
    'ProjectSettings/ProjectVersion.txt', 'ProjectSettings/EditorBuildSettings.asset'
)
$missing = @()
foreach ($relativePath in $required) {
    $path = Join-Path $ProjectPath $relativePath.Replace(
        '/', [System.IO.Path]::DirectorySeparatorChar
    )
    if (-not (Test-Path -LiteralPath $path)) {
        $missing += $relativePath
    }
}
if ($missing.Count -gt 0) {
    throw ("Unity bootstrap incomplete. Follow docs/getting-started.md. Missing: " + ($missing -join ', '))
}

$versionPath = Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
$version = [System.IO.File]::ReadAllText($versionPath)
if ($version -notmatch '(?m)^m_EditorVersion:\s*6000\.\d+\.\d+f\d+\s*$') {
    throw 'Pin an exact stable Unity 6 editor patch in ProjectVersion.txt.'
}

$manifest = Get-Content -LiteralPath (Join-Path $ProjectPath 'Packages\manifest.json') -Raw | ConvertFrom-Json
$lock = Get-Content -LiteralPath (Join-Path $ProjectPath 'Packages\packages-lock.json') -Raw | ConvertFrom-Json
foreach ($package in @('com.unity.render-pipelines.universal', 'com.unity.inputsystem', 'com.unity.test-framework')) {
    $entry = $manifest.dependencies.PSObject.Properties[$package]
    $locked = $lock.dependencies.PSObject.Properties[$package]
    if ($null -eq $entry -or $entry.Value -notmatch '^\d+\.\d+\.\d+(-[A-Za-z0-9.-]+)?$') {
        throw "Missing or unpinned required registry package: $package"
    }
    if ($null -eq $locked -or $locked.Value.version -ne $entry.Value) {
        throw "Package lock does not match manifest for $package."
    }
}

$assetsPath = Join-Path $ProjectPath 'Assets'
$assets = @(Get-ChildItem -LiteralPath $assetsPath -Recurse -Force)
foreach ($asset in $assets) {
    if ($asset.Extension -eq '.meta') {
        continue
    }
    if (-not (Test-Path -LiteralPath ($asset.FullName + '.meta') -PathType Leaf)) {
        throw "Unity asset/folder has no metadata file: $($asset.FullName)"
    }
}

$assemblies = @($assets | Where-Object { $_.Extension -eq '.asmdef' })
foreach ($name in @('GrindHero.Domain', 'GrindHero.Application')) {
    $matches = @($assemblies | Where-Object {
        (Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json).name -eq $name
    })
    if ($matches.Count -ne 1) {
        throw "Expected exactly one $name assembly definition."
    }
    $definition = Get-Content -LiteralPath $matches[0].FullName -Raw | ConvertFrom-Json
    $engineFlag = $definition.PSObject.Properties['noEngineReferences']
    if ($null -eq $engineFlag -or $engineFlag.Value -ne $true) {
        throw "$name must set noEngineReferences to true."
    }
}

foreach ($suite in @('EditMode', 'PlayMode')) {
    $testPath = Join-Path $assetsPath "_GrindHero\Tests\$suite"
    $testAssemblies = @(Get-ChildItem -LiteralPath $testPath -Filter '*.asmdef' -File -Recurse)
    $testCode = @(Get-ChildItem -LiteralPath $testPath -Filter '*.cs' -File -Recurse)
    if ($testAssemblies.Count -eq 0 -or $testCode.Count -eq 0) {
        throw "$suite requires a test assembly and actual test code."
    }
}

$buildSettings = Get-Content -LiteralPath (Join-Path $ProjectPath 'ProjectSettings\EditorBuildSettings.asset') -Raw
if ($buildSettings -notmatch '(?m)^\s*-\s+enabled:\s*1\s*$') {
    throw 'Enable a playable smoke scene in EditorBuildSettings before engine CI.'
}
Write-Output 'Unity bootstrap readiness checks passed; engine compilation/tests must still run.'
