# Build Waraq Windows installer for WRQ-WIN-002 (src/ WinUI 3)
# Requires: .NET 8 SDK, Inno Setup 6 (ISCC.exe), Windows SDK signtool

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "",
    [string]$PfxPath = "",
    [string]$PfxPassword = "",
    [switch]$SkipTests,
    [switch]$SkipSign,
    [switch]$GenerateCiCert
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $Root

function Find-ISCC {
    $candidates = @(
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c)) { return $c }
    }
    $cmd = Get-Command iscc -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

function Find-SignTool {
    $kits = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
    if (Test-Path $kits) {
        $found = Get-ChildItem -Path $kits -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($found) { return $found.FullName }
    }
    $cmd = Get-Command signtool -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $props = Join-Path $Root "src\Directory.Build.props"
    [xml]$xml = Get-Content $props
    $Version = @($xml.Project.PropertyGroup.Version | Where-Object { $_ }) | Select-Object -First 1
    if (-not $Version) { $Version = "0.9.0-phase9" }
}
$SafeVersion = ($Version -replace '[\\/:\s\+]', '-')

$PublishDir = Join-Path $Root "artifacts\publish"
$InstallerDir = Join-Path $Root "artifacts\installer"
$CertDir = Join-Path $Root "artifacts\certs"
New-Item -ItemType Directory -Force -Path $PublishDir, $InstallerDir, $CertDir | Out-Null

Write-Host "==> Version: $Version"
Write-Host "==> Publish: $PublishDir (src WinUI)"

Push-Location (Join-Path $Root "src")
try {
    dotnet restore Waraq.Windows.sln
    if ($LASTEXITCODE -ne 0) { throw "restore failed" }
    if (-not $SkipTests) {
        dotnet build Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) { throw "test build failed" }
        dotnet test Waraq.Windows.Tests/Waraq.Windows.Tests.csproj -c $Configuration --no-build --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "test failed" }
    }
    if (Test-Path $PublishDir) { Remove-Item -Recurse -Force $PublishDir }
    New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
    # Unpackaged WinUI + self-contained .NET + WindowsAppSDKSelfContained
    dotnet publish Waraq.Windows.App/Waraq.Windows.App.csproj `
        -c $Configuration `
        -r $Runtime `
        -p:Platform=x64 `
        --self-contained true `
        -p:WindowsPackageType=None `
        -p:WindowsAppSDKSelfContained=true `
        -p:PublishSingleFile=false `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -p:Version=$Version `
        -o $PublishDir
    if ($LASTEXITCODE -ne 0) { throw "publish failed" }
}
finally {
    Pop-Location
}

$AppExe = Join-Path $PublishDir "Waraq.Windows.App.exe"
if (-not (Test-Path $AppExe)) {
    $fallback = Get-ChildItem $PublishDir -Filter "*.exe" -Recurse | Where-Object { $_.Name -notmatch 'createdump|InstallUtil' } | Select-Object -First 1
    if ($fallback) {
        Write-Warning "Expected Waraq.Windows.App.exe missing; using $($fallback.FullName)"
        $AppExe = $fallback.FullName
    } else {
        throw "Missing app EXE after publish in $PublishDir"
    }
}

$workPfx = $PfxPath
$workPass = $PfxPassword
$cerOut = Join-Path $CertDir "WaraqWindows-CodeSigning.cer"

if ($GenerateCiCert -or ([string]::IsNullOrWhiteSpace($workPfx) -and -not $SkipSign)) {
    Write-Host "==> Generating ephemeral self-signed code signing certificate"
    $plain = -join ((48..57 + 65..90 + 97..122 | Get-Random -Count 24 | ForEach-Object { [char]$_ }))
    $secure = ConvertTo-SecureString -String $plain -Force -AsPlainText
    $subject = "CN=Waraq Windows OSS Self-Signed (WRQ-WIN-002), O=Waraq Windows contributors"
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $subject `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(3) `
        -FriendlyName "WaraqWindows-Phase9-CI"
    $workPfx = Join-Path $CertDir "waraq-codesign-ephemeral.pfx"
    Export-PfxCertificate -Cert $cert -FilePath $workPfx -Password $secure | Out-Null
    Export-Certificate -Cert $cert -FilePath $cerOut -Type CERT | Out-Null
    $workPass = $plain
    Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force -ErrorAction SilentlyContinue
    Write-Host "    CER: $cerOut"
    Write-Host "    Thumbprint: $($cert.Thumbprint)"
} elseif ((-not [string]::IsNullOrWhiteSpace($workPfx)) -and (Test-Path $workPfx) -and -not (Test-Path $cerOut)) {
    # Best-effort export public cert from PFX for artifact
    try {
        $secure = ConvertTo-SecureString -String $workPass -Force -AsPlainText
        $imported = Import-PfxCertificate -FilePath $workPfx -CertStoreLocation Cert:\CurrentUser\My -Password $secure
        Export-Certificate -Cert $imported -FilePath $cerOut -Type CERT | Out-Null
        Remove-Item -Path "Cert:\CurrentUser\My\$($imported.Thumbprint)" -Force -ErrorAction SilentlyContinue
    } catch {
        Write-Warning "Could not export CER from PFX: $_"
    }
}

function Invoke-Sign([string]$file) {
    if ($SkipSign) {
        Write-Host "SKIP sign: $file"
        return
    }
    if (-not $workPfx -or -not (Test-Path $workPfx)) { throw "No PFX available for signing $file" }
    $signtool = Find-SignTool
    if (-not $signtool) { throw "signtool.exe not found (install Windows SDK signing tools)" }
    Write-Host "==> Sign $file"
    & $signtool sign /f $workPfx /p $workPass /fd SHA256 /td SHA256 /tr http://timestamp.digicert.com $file
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Timestamp sign failed; retrying without timestamp"
        & $signtool sign /f $workPfx /p $workPass /fd SHA256 $file
        if ($LASTEXITCODE -ne 0) { throw "signtool failed for $file" }
    }
    $sig = Get-AuthenticodeSignature -FilePath $file
    Write-Host "    Status: $($sig.Status)  Signer: $($sig.SignerCertificate.Subject)"
}

Invoke-Sign $AppExe

$iscc = Find-ISCC
if (-not $iscc) { throw "Inno Setup 6 ISCC.exe not found. Install from https://jrsoftware.org/isinfo.php" }

$outputBase = "Waraq.Windows-Setup-win-x64-$SafeVersion"
$iss = Join-Path $Root "installer\waraq-windows.iss"
Write-Host "==> Compile installer with $iscc"
& $iscc `
    "/DMyAppVersion=$SafeVersion" `
    "/DMyAppSourceDir=$PublishDir" `
    "/DMyOutputDir=$InstallerDir" `
    "/DMyOutputBase=$outputBase" `
    $iss
if ($LASTEXITCODE -ne 0) { throw "ISCC failed" }

$setup = Join-Path $InstallerDir "$outputBase.exe"
if (-not (Test-Path $setup)) {
    $setup = Get-ChildItem $InstallerDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $setup -or -not (Test-Path $setup)) { throw "Setup.exe not found in $InstallerDir" }

Invoke-Sign $setup

$sumPath = Join-Path $InstallerDir "SHA256SUMS-installer.txt"
$lines = @()
Get-ChildItem $InstallerDir -File | ForEach-Object {
    $h = (Get-FileHash -Algorithm SHA256 $_.FullName).Hash.ToLower()
    $lines += "{0}  {1}" -f $h, $_.Name
}
if (Test-Path $cerOut) {
    $h = (Get-FileHash -Algorithm SHA256 $cerOut).Hash.ToLower()
    $lines += "{0}  {1}" -f $h, (Split-Path $cerOut -Leaf)
}
$lines | Set-Content -Path $sumPath -Encoding ascii

$report = Join-Path $InstallerDir "SIGNATURES.txt"
@(
    "Waraq Windows installer signature report (WRQ-WIN-002 Phase 9)",
    "Version: $Version",
    "App: $AppExe",
    "Generated: $(Get-Date -Format o)",
    "",
    "App EXE:",
    (Get-AuthenticodeSignature $AppExe | Format-List | Out-String),
    "Setup EXE:",
    (Get-AuthenticodeSignature $setup | Format-List | Out-String)
) | Set-Content -Path $report -Encoding utf8

Write-Host "OK Setup: $setup"
Write-Host "OK SUMS: $sumPath"
if (Test-Path $cerOut) { Write-Host "OK CER: $cerOut" }
