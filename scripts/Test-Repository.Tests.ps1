Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Test-Repository.ps1')

$rootPath = Split-Path -Parent $PSScriptRoot
$readmePath = Join-Path $rootPath 'README.md'
$cases = @(
    @{ Name = 'Existing relative link'; Content = '[Docs](docs/architecture.md)'; Expected = 0 },
    @{ Name = 'Existing link with anchor'; Content = '[Docs](docs/architecture.md#baseline)'; Expected = 0 },
    @{ Name = 'External links'; Content = '[Web](https://example.com/a) [Mail](mailto:dev@example.com)'; Expected = 0 },
    @{ Name = 'Anchor only'; Content = '[Here](#start-here)'; Expected = 0 },
    @{ Name = 'Missing target'; Content = '[Missing](docs/no-such-foundation-page.md)'; Expected = 1 },
    @{ Name = 'Repository escape'; Content = '[Outside](../outside.md)'; Expected = 1 },
    @{ Name = 'Encoded escape'; Content = '[Outside](%2e%2e/outside.md)'; Expected = 1 },
    @{ Name = 'Missing image'; Content = '![Image](docs/no-such-image.png)'; Expected = 1 },
    @{ Name = 'Link title'; Content = '[Docs](docs/architecture.md "Architecture")'; Expected = 0 },
    @{ Name = 'Fenced example'; Content = "``````markdown`n[Missing](not-real.md)`n```````n"; Expected = 0 }
)
foreach ($case in $cases) {
    $errors = @(Get-LocalMarkdownLinkErrors -RepositoryRoot $rootPath `
        -FilePath $readmePath -Content $case.Content)
    if ($errors.Count -ne $case.Expected) {
        throw "$($case.Name): expected $($case.Expected) errors, got $($errors.Count): $errors"
    }
}

$nestedErrors = @(Get-LocalMarkdownLinkErrors -RepositoryRoot $rootPath `
    -FilePath (Join-Path $rootPath 'docs\architecture.md') -Content '[Home](../README.md)')
if ($nestedErrors.Count -ne 0) {
    throw "Valid parent-relative link rejected: $nestedErrors"
}

$errors = @(Get-RepositoryErrors -RepositoryRoot $rootPath)
if ($errors.Count -ne 0) {
    throw ("Repository fixture is invalid:`n" + ($errors -join "`n"))
}
Write-Output "$($cases.Count + 2) foundation checker regression cases passed."
