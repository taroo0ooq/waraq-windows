# Package Waraq.Windows release zip (local mirror of CI)

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $Root

if ([string]::IsNullOrWhiteSpace($Version)) {
    $csproj = Join-Path $Root "archive\wrq-win-001\windows\Waraq.Windows\Waraq.Windows.csproj"
    [xml]$xml = Get-Content $csproj
    $Version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $Version) { $Version = "0.0.0-local" }
}

$Safe = ($Version -replace '[\\/:\s]', '-')
$ArtifactName = "Waraq.Windows-$Runtime-$Safe"
$PublishDir = Join-Path $Root "artifacts\publish"
$StageDir = Join-Path $Root "artifacts\stage\$ArtifactName"
$ZipPath = Join-Path $Root "artifacts\$ArtifactName.zip"

Write-Host "Root: $Root"
Write-Host "Version: $Version"
Write-Host "Artifact: $ArtifactName"

if (Test-Path (Join-Path $Root "artifacts")) {
    # keep other artifacts; clear publish/stage targets only
}
New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
if (Test-Path $StageDir) { Remove-Item -Recurse -Force $StageDir }
New-Item -ItemType Directory -Force -Path $StageDir | Out-Null

Push-Location (Join-Path $Root "archive\wrq-win-001\windows")
try {
    dotnet restore WaraqWindows.sln
    if ($LASTEXITCODE -ne 0) { throw "restore failed" }
    dotnet test WaraqWindows.sln -c $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "test failed" }
    dotnet publish Waraq.Windows\Waraq.Windows.csproj `
        -c $Configuration `
        -r $Runtime `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -p:Version=$Version `
        -o $PublishDir
    if ($LASTEXITCODE -ne 0) { throw "publish failed" }
}
finally {
    Pop-Location
}

Copy-Item -Path (Join-Path $PublishDir "*") -Destination $StageDir -Recurse -Force
Copy-Item (Join-Path $Root "LICENSE") $StageDir
Copy-Item (Join-Path $Root "NOTICE") $StageDir
Copy-Item (Join-Path $Root "docs\install\WINDOWS.md") (Join-Path $StageDir "README-INSTALL.md")

if (Test-Path $ZipPath) { Remove-Item -Force $ZipPath }
Compress-Archive -Path (Join-Path $StageDir "*") -DestinationPath $ZipPath -Force

$hash = (Get-FileHash -Algorithm SHA256 $ZipPath).Hash.ToLower()
$sums = Join-Path $Root "artifacts\SHA256SUMS.txt"
Set-Content -Path $sums -Value ("{0}  {1}" -f $hash, (Split-Path $ZipPath -Leaf)) -Encoding ascii

Write-Host "OK: $ZipPath"
Write-Host "SHA256: $hash"
Write-Host "SUMS: $sums"
