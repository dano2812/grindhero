param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-LocalMarkdownLinkErrors {
    param(
        [string]$RepositoryRoot,
        [string]$FilePath,
        [string]$Content
    )

    $rootPath = [System.IO.Path]::GetFullPath($RepositoryRoot)
    $rootPrefix = $rootPath.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $comparison = [System.StringComparison]::Ordinal
    if ([System.IO.Path]::DirectorySeparatorChar -eq '\') {
        $comparison = [System.StringComparison]::OrdinalIgnoreCase
    }

    # Ignore illustrative Markdown inside fenced code blocks.
    $markdown = [regex]::Replace($Content, '(?ms)^```[^\r\n]*\r?\n.*?^```[^\r\n]*', '')
    foreach ($match in [regex]::Matches($markdown, '\[[^\]\r\n]+\]\(([^)\r\n]+)\)')) {
        $target = $match.Groups[1].Value.Trim()
        if ($target -match '^(https?://|mailto:|#)') {
            continue
        }

        $target = ($target -split '\s+"', 2)[0].Trim('<', '>')
        $target = ($target -split '[#?]', 2)[0]
        if ([string]::IsNullOrWhiteSpace($target)) {
            continue
        }

        $target = [System.Uri]::UnescapeDataString($target)
        $target = $target.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
        $target = $target.Replace('\', [System.IO.Path]::DirectorySeparatorChar)
        if ([System.IO.Path]::IsPathRooted($target)) {
            "Rooted Markdown link in ${FilePath}: $target"
            continue
        }

        $resolved = [System.IO.Path]::GetFullPath(
            (Join-Path (Split-Path -Parent $FilePath) $target)
        )
        if (-not $resolved.StartsWith($rootPrefix, $comparison)) {
            "Markdown link escapes repository in ${FilePath}: $target"
        } elseif (-not (Test-Path -LiteralPath $resolved)) {
            "Broken Markdown link in ${FilePath}: $target"
        }
    }
}

function Get-RepositoryErrors {
    param([string]$RepositoryRoot)

    $required = @(
        'README.md', 'CONTRIBUTING.md', 'SECURITY.md',
        '.gitignore', '.gitattributes', '.editorconfig',
        'game/README.md',
        'docs/game-design.md', 'docs/architecture.md', 'docs/getting-started.md',
        'docs/gameplay-specification.md',
        'docs/development.md', 'docs/testing.md', 'docs/security.md',
        'docs/delivery.md', 'docs/roadmap.md', 'docs/assets.md',
        'docs/adr/README.md', 'docs/adr/0001-unity-mobile-foundation.md',
        '.github/github-app.yml', '.github/PULL_REQUEST_TEMPLATE.md',
        '.github/ISSUE_TEMPLATE/task.yml', '.github/ISSUE_TEMPLATE/bug.yml',
        '.github/workflows/repository.yml', '.github/workflows/unity-tests.yml',
        '.github/dependabot.yml',
        'scripts/Test-Repository.ps1', 'scripts/Test-Repository.Tests.ps1',
        'scripts/Test-UnityChecks.Tests.ps1',
        'scripts/Test-UnityProjectReady.ps1', 'scripts/Assert-UnityTestResults.ps1'
    )

    foreach ($relativePath in $required) {
        $path = Join-Path $RepositoryRoot $relativePath.Replace(
            '/', [System.IO.Path]::DirectorySeparatorChar
        )
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            "Missing required file: $relativePath"
        } elseif ([string]::IsNullOrWhiteSpace([System.IO.File]::ReadAllText($path))) {
            "Empty required file: $relativePath"
        }
    }

    $markdownFiles = @(Get-ChildItem -LiteralPath $RepositoryRoot -Filter '*.md' -File)
    foreach ($folder in @('docs', 'game', '.github')) {
        $path = Join-Path $RepositoryRoot $folder
        if (Test-Path -LiteralPath $path -PathType Container) {
            if ($folder -eq 'game') {
                # Do not scan generated caches or vendor docs after Unity bootstrap.
                $markdownFiles += Get-ChildItem -LiteralPath $path -Filter '*.md' -File
            } else {
                $markdownFiles += Get-ChildItem -LiteralPath $path -Filter '*.md' -File -Recurse
            }
        }
    }
    foreach ($file in $markdownFiles) {
        Get-LocalMarkdownLinkErrors -RepositoryRoot $RepositoryRoot `
            -FilePath $file.FullName -Content ([System.IO.File]::ReadAllText($file.FullName))
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $Root = (Resolve-Path -LiteralPath $Root).Path
    $errors = @(Get-RepositoryErrors -RepositoryRoot $Root)
    if ($errors.Count -gt 0) {
        throw ("Repository checks failed:`n" + ($errors -join "`n"))
    }
    Write-Output 'Repository foundation checks passed (required files and local Markdown links).'
}
