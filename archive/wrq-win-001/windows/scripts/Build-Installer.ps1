# Build Waraq Windows installer (publish + optional sign + Inno Setup)
# Requires: .NET 8 SDK, Inno Setup 6 (ISCC.exe), Windows SDK signtool (for signing)

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
    $csproj = Join-Path $Root "archive\wrq-win-001\windows\Waraq.Windows\Waraq.Windows.csproj"
    [xml]$xml = Get-Content $csproj
    $Version = @($xml.Project.PropertyGroup.Version | Where-Object { $_ }) | Select-Object -First 1
    if (-not $Version) { $Version = "0.2.0-alpha" }
}
$SafeVersion = ($Version -replace '[\\/:\s\+]', '-')

$PublishDir = Join-Path $Root "artifacts\publish"
$InstallerDir = Join-Path $Root "artifacts\installer"
$CertDir = Join-Path $Root "artifacts\certs"
New-Item -ItemType Directory -Force -Path $PublishDir, $InstallerDir, $CertDir | Out-Null

Write-Host "==> Version: $Version"
Write-Host "==> Publish: $PublishDir"

Push-Location (Join-Path $Root "archive\wrq-win-001\windows")
try {
    dotnet restore WaraqWindows.sln
    if ($LASTEXITCODE -ne 0) { throw "restore failed" }
    if (-not $SkipTests) {
        dotnet test WaraqWindows.sln -c $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "test failed" }
    }
    if (Test-Path $PublishDir) { Remove-Item -Recurse -Force $PublishDir }
    New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
    dotnet publish Waraq.Windows\Waraq.Windows.csproj `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
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

# Folder publish (not single-file) plays nicer with Inno + signing individual EXE
$AppExe = Join-Path $PublishDir "Waraq.Windows.exe"
if (-not (Test-Path $AppExe)) { throw "Missing $AppExe after publish" }

# Signing cert
$workPfx = $PfxPath
$workPass = $PfxPassword
$cerOut = Join-Path $CertDir "WaraqWindows-CodeSigning.cer"

if ($GenerateCiCert -or ($workPfx -eq "" -and -not $SkipSign)) {
    Write-Host "==> Generating ephemeral self-signed code signing certificate"
    $plain = -join ((48..57 + 65..90 + 97..122 | Get-Random -Count 24 | ForEach-Object { [char]$_ }))
    $secure = ConvertTo-SecureString -String $plain -Force -AsPlainText
    $subject = "CN=Waraq Windows OSS Self-Signed (WRQ-WIN-001), O=Waraq Windows contributors"
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $subject `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddYears(3) `
        -FriendlyName "WaraqWindows-Phase6-CI"
    $workPfx = Join-Path $CertDir "waraq-codesign-ephemeral.pfx"
    Export-PfxCertificate -Cert $cert -FilePath $workPfx -Password $secure | Out-Null
    Export-Certificate -Cert $cert -FilePath $cerOut -Type CERT | Out-Null
    $workPass = $plain
    # remove from store (keep PFX only for this job)
    Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force -ErrorAction SilentlyContinue
    Write-Host "    CER: $cerOut"
    Write-Host "    Thumbprint: $($cert.Thumbprint)"
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
    $args = @("sign", "/f", $workPfx, "/p", $workPass, "/fd", "SHA256", "/td", "SHA256", "/tr", "http://timestamp.digicert.com", $file)
    & $signtool @args
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Timestamp sign failed; retrying without timestamp"
        $args = @("sign", "/f", $workPfx, "/p", $workPass, "/fd", "SHA256", $file)
        & $signtool @args
        if ($LASTEXITCODE -ne 0) { throw "signtool failed for $file" }
    }
    $sig = Get-AuthenticodeSignature -FilePath $file
    Write-Host "    Status: $($sig.Status)  Signer: $($sig.SignerCertificate.Subject)"
    if ($sig.Status -ne "Valid" -and $sig.Status -ne "UnknownError") {
        # Self-signed often shows UnknownError/NotTrusted until CER is trusted — Status can be UnknownError
        Write-Host "    Note: self-signed may report NotTrusted/Unknown until CER imported (expected)."
    }
}

Invoke-Sign $AppExe

$iscc = Find-ISCC
if (-not $iscc) { throw "Inno Setup 6 ISCC.exe not found. Install from https://jrsoftware.org/isinfo.php" }

$outputBase = "Waraq.Windows-Setup-win-x64-$SafeVersion"
$iss = Join-Path $Root "archive\wrq-win-001\installer\waraq-windows.iss"
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
    # Inno may sanitize version differently — pick newest exe in output dir
    $setup = Get-ChildItem $InstallerDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $setup -or -not (Test-Path $setup)) { throw "Setup.exe not found in $InstallerDir" }

Invoke-Sign $setup

# Checksums
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

# Signature report
$report = Join-Path $InstallerDir "SIGNATURES.txt"
@(
    "Waraq Windows installer signature report",
    "Version: $Version",
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
