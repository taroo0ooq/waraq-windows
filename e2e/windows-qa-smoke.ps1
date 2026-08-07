# WRQ-WIN-002 Phase 3-QA — Windows QA smoke (Stagecraft QA)
# Builds src/ WinUI solution, runs unit tests, launches app briefly, writes evidence.
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$SrcDir = Join-Path $RepoRoot 'src'
$OutDir = Join-Path $RepoRoot 'e2e/out'
$FixtureDir = Join-Path $RepoRoot 'e2e/fixtures'
$Solution = Join-Path $SrcDir 'Waraq.Windows.sln'
$TestProj = Join-Path $SrcDir 'Waraq.Windows.Tests/Waraq.Windows.Tests.csproj'
$AppProj = Join-Path $SrcDir 'Waraq.Windows.App/Waraq.Windows.App.csproj'

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
New-Item -ItemType Directory -Force -Path $FixtureDir | Out-Null

$started = Get-Date
$log = Join-Path $OutDir 'smoke-log.txt'
if (Test-Path $log) { Remove-Item $log -Force }

function Write-Log {
    param([string]$Message)
    $line = '[{0:u}] {1}' -f (Get-Date), $Message
    Write-Host $line
    Add-Content -Path $log -Value $line
}

Write-Log "WRQ-WIN-002 Phase 3-QA Windows smoke starting (Configuration=$Configuration)"
Write-Log "RepoRoot=$RepoRoot"

$gif = Join-Path $FixtureDir 'sample.gif'
$mp4 = Join-Path $FixtureDir 'sample.mp4'

function Ensure-FfmpegFixture {
    param(
        [string]$OutPath,
        [string[]]$FfmpegArgs
    )
    if (Test-Path $OutPath) {
        Write-Log "Fixture exists: $OutPath"
        return
    }
    $ffmpeg = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if (-not $ffmpeg) {
        Write-Log "WARNING: ffmpeg not on PATH - skipping $OutPath"
        return
    }
    Write-Log "Generating fixture: $OutPath"
    & ffmpeg -y @FfmpegArgs $OutPath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $OutPath)) {
        throw "ffmpeg failed generating $OutPath (exit=$LASTEXITCODE)"
    }
}

Ensure-FfmpegFixture -OutPath $gif -FfmpegArgs @(
    '-f', 'lavfi', '-i', 'color=c=red:s=64x64:d=0.6',
    '-frames:v', '6', '-gifflags', '+transdiff'
)
Ensure-FfmpegFixture -OutPath $mp4 -FfmpegArgs @(
    '-f', 'lavfi', '-i', 'color=c=blue:s=320x240:d=1',
    '-pix_fmt', 'yuv420p', '-t', '1'
)

$env:WARAQ_QA_FIXTURE_DIR = $FixtureDir
Write-Log "WARAQ_QA_FIXTURE_DIR=$FixtureDir"

Push-Location $SrcDir
try {
    Write-Log 'dotnet restore'
    dotnet restore $Solution
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed' }

    Write-Log "dotnet build tests -c $Configuration"
    dotnet build $TestProj -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build tests failed' }

    Write-Log "dotnet test -c $Configuration"
    $trxName = 'phase3-qa-results.trx'
    dotnet test $TestProj -c $Configuration --no-build --verbosity normal --logger "trx;LogFileName=$trxName" --logger 'console;verbosity=detailed'
    if ($LASTEXITCODE -ne 0) { throw 'dotnet test failed' }

    Write-Log "dotnet build WinUI App x64 -c $Configuration"
    dotnet build $AppProj -c $Configuration -p:Platform=x64 --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build app failed' }

    $exeCandidates = @(
        (Join-Path $SrcDir "Waraq.Windows.App/bin/x64/$Configuration/net8.0-windows10.0.19041.0/Waraq.Windows.App.exe"),
        (Join-Path $SrcDir "Waraq.Windows.App/bin/x64/$Configuration/net8.0-windows10.0.19041.0/win-x64/Waraq.Windows.App.exe")
    )
    $exe = $exeCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $exe) {
        $found = Get-ChildItem -Path (Join-Path $SrcDir 'Waraq.Windows.App/bin') -Recurse -Filter 'Waraq.Windows.App.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($found) { $exe = $found.FullName }
    }
    if (-not $exe -or -not (Test-Path $exe)) {
        throw "WinUI exe missing after build under src/Waraq.Windows.App/bin"
    }
    Write-Log "Exe OK: $exe"
    Copy-Item $exe (Join-Path $OutDir 'Waraq.Windows.App.exe') -Force

    Write-Log 'Launch smoke (4s)...'
    $proc = Start-Process -FilePath $exe -PassThru -WindowStyle Minimized
    Start-Sleep -Seconds 4
    if ($proc.HasExited) {
        throw "Waraq.Windows.App.exe exited early with code $($proc.ExitCode)"
    }
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    Write-Log 'Launch smoke OK (process was alive then stopped)'
}
finally {
    Pop-Location
}

$elapsed = (Get-Date) - $started
$seconds = [int]$elapsed.TotalSeconds
$summaryPath = Join-Path $OutDir 'smoke-summary.md'
$summary = @"
# Windows QA smoke summary

- work_id: WRQ-WIN-002
- phase: 3-QA
- desk: Stagecraft QA
- configuration: $Configuration
- elapsed_seconds: $seconds
- solution: src/Waraq.Windows.sln
- fixtures: sample.gif / sample.mp4
- tests: Waraq.Windows.Tests
- launch: Waraq.Windows.App.exe 4s alive
- result: PASS

## Residual risk (explicit)

- Browser Playwright: N/A (WinUI desktop; no web UI).
- Full interactive Apply (file picker + visual wallpaper proof): soft residual for owner L&F; path gate + host probe + size caps automated.
- Tray Stop wallpaper is code-reviewed (CMD_PAUSE -> App.Wallpaper.Stop); not UI-automateable without FlaUI.
- Video decode depends on host Media Foundation codecs.
- WorkerW/Explorer variance may require re-Apply after Explorer restart.
- Multi-monitor: one virtual-desktop surface (MVP note).

## Evidence

- Log: e2e/out/smoke-log.txt
- TRX under src/**/TestResults/
"@
Set-Content -Path $summaryPath -Value $summary -Encoding UTF8
Write-Log "PASS - summary at $summaryPath"
exit 0
