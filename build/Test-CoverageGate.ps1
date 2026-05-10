param(
    [string]$Configuration = "Release",
    [double]$Threshold = 80,
    [string]$ResultsDirectory = "artifacts\coverage",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repoRoot "src\ElBruno.AspireMonitor.Tests\ElBruno.AspireMonitor.Tests.csproj"
$resultsPath = Join-Path $repoRoot $ResultsDirectory

if (Test-Path $resultsPath) {
    Remove-Item $resultsPath -Recurse -Force
}
New-Item -ItemType Directory -Path $resultsPath | Out-Null

$testArgs = @(
    "test",
    $testProject,
    "-c", $Configuration,
    "--no-restore",
    "--verbosity", "minimal"
)

if ($NoBuild) {
    $testArgs += "--no-build"
}

$testArgs += @(
    "--collect", "XPlat Code Coverage",
    "--results-directory", $resultsPath,
    "--",
    "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura"
)

Write-Host "Running coverage tests..."
Write-Host "dotnet $($testArgs -join ' ')"
& dotnet @testArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$coverageFile = Get-ChildItem -Path $resultsPath -Filter "coverage.cobertura.xml" -Recurse |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $coverageFile) {
    throw "Coverage report not found under $resultsPath"
}

[xml]$coverage = Get-Content $coverageFile.FullName
$classes = @($coverage.coverage.packages.package.classes.class)

$includedClasses = $classes | Where-Object {
    $fileName = $_.filename
    $fileName -match '(^|[\\/])(Services|Models)[\\/]' -and
        $fileName -notmatch '(^|[\\/])(bin|obj)[\\/]'
}

$fileCoverage = @{}

foreach ($class in $includedClasses) {
    $file = $class.filename
    if (-not $fileCoverage.ContainsKey($file)) {
        $fileCoverage[$file] = @{}
    }

    $lines = @($class.lines.line)
    foreach ($line in $lines) {
        $lineNumber = [int]$line.number
        $hits = [int]$line.hits

        if (-not $fileCoverage[$file].ContainsKey($lineNumber)) {
            $fileCoverage[$file][$lineNumber] = 0
        }

        if ($hits -gt $fileCoverage[$file][$lineNumber]) {
            $fileCoverage[$file][$lineNumber] = $hits
        }
    }
}

$coveredLines = 0
$totalLines = 0
$classRows = foreach ($file in $fileCoverage.Keys) {
    $fileLines = $fileCoverage[$file]
    if ($fileLines.Count -eq 0) {
        continue
    }

    $fileCovered = @($fileLines.Values | Where-Object { $_ -gt 0 }).Count
    $fileTotal = $fileLines.Count
    $coveredLines += $fileCovered
    $totalLines += $fileTotal

    [pscustomobject]@{
        File = $file
        Covered = $fileCovered
        Total = $fileTotal
        Percent = [math]::Round(($fileCovered / $fileTotal) * 100, 2)
    }
}

if ($totalLines -eq 0) {
    throw "No coverable lines found for Services/Models. Check coverage instrumentation."
}

$coveragePercent = [math]::Round(($coveredLines / $totalLines) * 100, 2)

Write-Host ""
Write-Host "Services/Models coverage:"
$classRows |
    Sort-Object Percent, File |
    Format-Table File, Covered, Total, Percent -AutoSize

Write-Host ("Services/Models line coverage: {0}/{1} = {2}%" -f $coveredLines, $totalLines, $coveragePercent)
Write-Host ("Required threshold: {0}%" -f $Threshold)

if ($coveragePercent -lt $Threshold) {
    throw ("Coverage gate failed: {0}% is below {1}%" -f $coveragePercent, $Threshold)
}

Write-Host ("Coverage gate passed: {0}% >= {1}%" -f $coveragePercent, $Threshold)
