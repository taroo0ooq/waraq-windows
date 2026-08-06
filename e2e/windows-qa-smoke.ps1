# WRQ-WIN-001 Phase 4 — Windows QA smoke (Stagecraft QA)
# Builds Release, generates fixtures, runs unit + STA integration tests, writes evidence.
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$WindowsDir = Join-Path $RepoRoot 'windows'
$OutDir = Join-Path $RepoRoot 'e2e/out'
$FixtureDir = Join-Path $RepoRoot 'e2e/fixtures'
$Solution = Join-Path $WindowsDir 'WaraqWindows.sln'

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

Write-Log "WRQ-WIN-001 Phase 4 Windows QA smoke starting (Configuration=$Configuration)"
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
        Write-Log "WARNING: ffmpeg not on PATH - skipping generation of $OutPath"
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

Push-Location $WindowsDir
try {
    Write-Log 'dotnet restore'
    dotnet restore $Solution
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed' }

    Write-Log "dotnet build -c $Configuration"
    dotnet build $Solution -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

    $trxName = 'phase4-qa-results.trx'
    Write-Log "dotnet test -c $Configuration"
    dotnet test $Solution -c $Configuration --no-build --verbosity normal --logger "trx;LogFileName=$trxName" --logger 'console;verbosity=detailed'
    if ($LASTEXITCODE -ne 0) { throw 'dotnet test failed' }

    $exe = Join-Path $WindowsDir "Waraq.Windows/bin/$Configuration/net8.0-windows/Waraq.Windows.exe"
    if (-not (Test-Path $exe)) {
        throw "Release exe missing: $exe"
    }
    Write-Log "Exe OK: $exe"
    Copy-Item $exe (Join-Path $OutDir 'Waraq.Windows.exe') -Force

    Write-Log 'Launch smoke (3s)...'
    $proc = Start-Process -FilePath $exe -PassThru -WindowStyle Minimized
    Start-Sleep -Seconds 3
    if ($proc.HasExited) {
        throw "Waraq.Windows.exe exited early with code $($proc.ExitCode)"
    }
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    Write-Log 'Launch smoke OK (process was alive, then stopped)'
}
finally {
    Pop-Location
}

$elapsed = (Get-Date) - $started
$seconds = [int]$elapsed.TotalSeconds
$summaryPath = Join-Path $OutDir 'smoke-summary.md'

$summary = @"
# Windows QA smoke summary

- work_id: WRQ-WIN-001
- phase: 4 Playwright/QA
- desk: Stagecraft QA
- configuration: $Configuration
- elapsed_seconds: $seconds
- fixtures: sample.gif / sample.mp4 (when ffmpeg available)
- tests: dotnet test WaraqWindows.sln
- launch: Waraq.Windows.exe 3s alive
- result: PASS

## Residual risk (explicit)

- Browser Playwright: N/A (no web UI).
- Full UI automation (WinAppDriver/FlaUI Browse dialog): deferred — path gate + STA Apply/Stop covered in tests; OpenFileDialog UX is manual matrix.
- Video decode depends on host Media Foundation codecs.
- Explorer/WorkerW layout quirks may require re-Apply after Explorer restart.

## Evidence

- Log: e2e/out/smoke-log.txt
- TRX under windows/**/TestResults/
"@

Set-Content -Path $summaryPath -Value $summary -Encoding UTF8
Write-Log "PASS - summary at $summaryPath"
exit 0
