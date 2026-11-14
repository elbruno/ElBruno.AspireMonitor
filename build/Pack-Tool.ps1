param(
    [string]$Configuration = 'Release',
    [string]$Version = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolProject = Join-Path $repoRoot 'src\ElBruno.AspireMonitor.Tool\ElBruno.AspireMonitor.Tool.csproj'
$desktopProject = Join-Path $repoRoot 'src\ElBruno.AspireMonitor\ElBruno.AspireMonitor.csproj'
$packageDirectory = Join-Path $repoRoot 'artifacts\packages'
$targets = @(
    @{ DesktopTfm = 'net10.0-windows'; ToolTfm = 'net10.0' },
    @{ DesktopTfm = 'net11.0-windows'; ToolTfm = 'net11.0' }
)

$versionArgs = @()
if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $versionArgs += "-p:Version=$Version"
}

# 1) Publish the WPF payload separately for each supported tool runtime.
foreach ($target in $targets) {
    $desktopPublishDir = Join-Path $repoRoot "artifacts\desktop-publish\$($target.DesktopTfm)\"
    dotnet publish $desktopProject -c $Configuration -f $target.DesktopTfm -r win-x64 --self-contained false -p:PublishDir="$desktopPublishDir" @versionArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for desktop target $($target.DesktopTfm)" }
}

# 2) Pack the multi-target tool project (produces a .nupkg without desktop payloads).
dotnet pack $toolProject -c $Configuration @versionArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for tool project" }

# 3) Find the freshly produced tool package.
$packagePath = Get-ChildItem -LiteralPath $packageDirectory -Filter 'ElBruno.AspireMonitor.*.nupkg' |
    Where-Object { $_.Name -notlike '*.symbols.nupkg' } |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $packagePath) { throw "Could not locate produced .nupkg in $packageDirectory" }

# 4) Inject each matching desktop payload into tools/<tfm>/any/desktop/ in the .nupkg.
foreach ($target in $targets) {
    $desktopPublishDir = Join-Path $repoRoot "artifacts\desktop-publish\$($target.DesktopTfm)\"
    & (Join-Path $PSScriptRoot 'Inject-DesktopPayload.ps1') -PackagePath $packagePath -DesktopPublishDir $desktopPublishDir -ToolTargetFramework $target.ToolTfm
}

Write-Host "Packaged tool: $packagePath"
