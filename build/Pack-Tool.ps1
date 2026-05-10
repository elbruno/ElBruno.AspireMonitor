param(
    [string]$Configuration = 'Release',
    [string]$Version = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolProject = Join-Path $repoRoot 'src\ElBruno.AspireMonitor.Tool\ElBruno.AspireMonitor.Tool.csproj'
$desktopProject = Join-Path $repoRoot 'src\ElBruno.AspireMonitor\ElBruno.AspireMonitor.csproj'
$desktopPublishDir = Join-Path $repoRoot 'src\ElBruno.AspireMonitor\obj\desktop-publish\'
$packageDirectory = Join-Path $repoRoot 'artifacts\packages'
$desktopTfm = 'net10.0-windows'
$toolTfm = 'net10.0'

$versionArgs = @()
if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $versionArgs += "-p:Version=$Version"
}

# 1) Publish the WPF desktop app to a known folder (framework-dependent, win-x64).
dotnet publish $desktopProject -c $Configuration -f $desktopTfm -r win-x64 --self-contained false -p:PublishDir="$desktopPublishDir" @versionArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for desktop app" }

# 2) Pack the tool project (produces a .nupkg with no desktop payload yet).
dotnet pack $toolProject -c $Configuration @versionArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for tool project" }

# 3) Find the freshly produced tool package.
$packagePath = Get-ChildItem -LiteralPath $packageDirectory -Filter 'ElBruno.AspireMonitor.*.nupkg' |
    Where-Object { $_.Name -notlike '*.symbols.nupkg' } |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $packagePath) { throw "Could not locate produced .nupkg in $packageDirectory" }

# 4) Inject the desktop payload into tools/<tfm>/any/desktop/ inside the .nupkg.
& (Join-Path $PSScriptRoot 'Inject-DesktopPayload.ps1') -PackagePath $packagePath -DesktopPublishDir $desktopPublishDir -ToolTargetFramework $toolTfm

Write-Host "Packaged tool: $packagePath"
