param(
    [Parameter(Mandatory = $true)]
    [string]$ResultsPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$files = @(Get-ChildItem -LiteralPath $ResultsPath -Filter '*.xml' -File -Recurse)
$testCount = 0
$passedCount = 0
foreach ($file in $files) {
    $document = New-Object System.Xml.XmlDocument
    # Reports are produced by executed test code; reject DTDs and external entities.
    $settings = New-Object System.Xml.XmlReaderSettings
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($file.FullName, $settings)
    try {
        $document.Load($reader)
    } finally {
        $reader.Dispose()
    }
    $cases = @($document.SelectNodes('//test-case'))
    foreach ($case in $cases) {
        $testCount++
        $result = $case.GetAttribute('result')
        if ($result -eq 'Failed') {
            throw "Failed Unity test in $($file.Name): $($case.GetAttribute('fullname'))"
        }
        if ($result -eq 'Passed') {
            $passedCount++
        }
    }
}
if ($testCount -eq 0 -or $passedCount -eq 0) {
    throw "No executed passing Unity tests found in $ResultsPath; bootstrap/discovery is incomplete."
}
Write-Output "Unity reports contain $testCount test cases and $passedCount passing cases."
