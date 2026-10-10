Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$rootPath = Split-Path -Parent $PSScriptRoot
$fixture = [System.IO.Path]::GetFullPath(
    (Join-Path $rootPath ('.artifacts\unity-check-fixture-' + [guid]::NewGuid().ToString('N')))
)
$project = Join-Path $fixture 'game'
$reports = Join-Path $fixture 'reports'
$readyScript = Join-Path $PSScriptRoot 'Test-UnityProjectReady.ps1'
$reportScript = Join-Path $PSScriptRoot 'Assert-UnityTestResults.ps1'
$caseCount = 0

function Write-FixtureFile {
    param([string]$RelativePath, [string]$Content)
    $path = Join-Path $project $RelativePath
    New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
    [System.IO.File]::WriteAllText($path, $Content)
}

function Assert-Rejected {
    param([scriptblock]$Action, [string]$ExpectedMessage)
    $rejected = $false
    try {
        & $Action | Out-Null
    } catch {
        if ($_.Exception.Message -notlike "*$ExpectedMessage*") {
            throw "Unexpected rejection: $($_.Exception.Message); expected $ExpectedMessage"
        }
        $rejected = $true
    }
    if (-not $rejected) {
        throw "Check accepted invalid fixture; expected $ExpectedMessage"
    }
}

try {
    New-Item -ItemType Directory -Path $fixture -Force | Out-Null
    Assert-Rejected { & $readyScript -ProjectPath $project } 'Unity bootstrap incomplete'
    $caseCount++

    # Synthetic structure tests the gate only; it is not a runnable Unity project.
    Write-FixtureFile 'ProjectSettings\ProjectVersion.txt' 'm_EditorVersion: 6000.3.0f1'
    Write-FixtureFile 'ProjectSettings\EditorBuildSettings.asset' "m_Scenes:`n  - enabled: 1"
    $manifest = '{"dependencies":{"com.unity.render-pipelines.universal":"17.3.0","com.unity.inputsystem":"1.14.0","com.unity.test-framework":"1.6.0"}}'
    $lock = '{"dependencies":{"com.unity.render-pipelines.universal":{"version":"17.3.0"},"com.unity.inputsystem":{"version":"1.14.0"},"com.unity.test-framework":{"version":"1.6.0"}}}'
    Write-FixtureFile 'Packages\manifest.json' $manifest
    Write-FixtureFile 'Packages\packages-lock.json' $lock
    Write-FixtureFile 'Assets\_GrindHero\Code\Domain\Domain.asmdef' '{"name":"GrindHero.Domain","noEngineReferences":true}'
    Write-FixtureFile 'Assets\_GrindHero\Code\Application\Application.asmdef' '{"name":"GrindHero.Application","noEngineReferences":true}'
    foreach ($suite in @('EditMode', 'PlayMode')) {
        Write-FixtureFile "Assets\_GrindHero\Tests\$suite\Tests.asmdef" "{`"name`":`"$suite`"}"
        Write-FixtureFile "Assets\_GrindHero\Tests\$suite\Sample.cs" '// Structural fixture only.'
    }
    $assetsPath = Join-Path $project 'Assets'
    foreach ($asset in @(Get-ChildItem -LiteralPath $assetsPath -Recurse)) {
        [System.IO.File]::WriteAllText($asset.FullName + '.meta', 'fileFormatVersion: 2')
    }
    & $readyScript -ProjectPath $project | Out-Null
    $caseCount++

    $sampleMeta = Join-Path $assetsPath '_GrindHero\Tests\EditMode\Sample.cs.meta'
    Remove-Item -LiteralPath $sampleMeta
    Assert-Rejected { & $readyScript -ProjectPath $project } 'has no metadata file'
    [System.IO.File]::WriteAllText($sampleMeta, 'fileFormatVersion: 2')
    $caseCount++

    Write-FixtureFile 'Packages\packages-lock.json' ($lock.Replace('17.3.0', '17.2.0'))
    Assert-Rejected { & $readyScript -ProjectPath $project } 'lock does not match'
    Write-FixtureFile 'Packages\packages-lock.json' $lock
    $caseCount++

    Write-FixtureFile 'Assets\_GrindHero\Code\Domain\Domain.asmdef' '{"name":"GrindHero.Domain","noEngineReferences":false}'
    Assert-Rejected { & $readyScript -ProjectPath $project } 'noEngineReferences'
    Write-FixtureFile 'Assets\_GrindHero\Code\Domain\Domain.asmdef' '{"name":"GrindHero.Domain","noEngineReferences":true}'
    $caseCount++

    Write-FixtureFile 'ProjectSettings\ProjectVersion.txt' 'm_EditorVersion: 6000.3.0b1'
    Assert-Rejected { & $readyScript -ProjectPath $project } 'exact stable Unity 6'
    Write-FixtureFile 'ProjectSettings\ProjectVersion.txt' 'm_EditorVersion: 6000.3.0f1'
    $caseCount++

    Write-FixtureFile 'ProjectSettings\EditorBuildSettings.asset' "m_Scenes:`n  - enabled: 0"
    Assert-Rejected { & $readyScript -ProjectPath $project } 'Enable a playable smoke scene'
    $caseCount++

    New-Item -ItemType Directory -Path $reports -Force | Out-Null
    $xmlPath = Join-Path $reports 'results.xml'
    [System.IO.File]::WriteAllText($xmlPath, '<test-run><test-case fullname="Sample" result="Passed" /></test-run>')
    & $reportScript -ResultsPath $reports | Out-Null
    $caseCount++

    [System.IO.File]::WriteAllText($xmlPath, '<test-run><test-case fullname="Sample" result="Failed" /></test-run>')
    Assert-Rejected { & $reportScript -ResultsPath $reports } 'Failed Unity test'
    $caseCount++

    [System.IO.File]::WriteAllText($xmlPath, '<test-run />')
    Assert-Rejected { & $reportScript -ResultsPath $reports } 'No executed passing Unity tests'
    $caseCount++

    [System.IO.File]::WriteAllText($xmlPath, '<test-run><test-case fullname="Sample" result="Skipped" /></test-run>')
    Assert-Rejected { & $reportScript -ResultsPath $reports } 'No executed passing Unity tests'
    $caseCount++

    [System.IO.File]::WriteAllText($xmlPath, '<!DOCTYPE test-run [<!ENTITY test "unsafe">]><test-run>&test;</test-run>')
    Assert-Rejected { & $reportScript -ResultsPath $reports } 'DTD is prohibited'
    $caseCount++
} finally {
    if (Test-Path -LiteralPath $fixture -PathType Container) {
        Remove-Item -LiteralPath $fixture -Recurse -Force
    }
}

Write-Output "$caseCount Unity readiness/report regression cases passed (no editor required)."
