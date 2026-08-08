param(
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Repo = "nodesire7/CreamInstaller"
$Root = $PSScriptRoot
$Project = Join-Path $Root "CreamInstaller\CreamInstaller.csproj"
$Solution = Join-Path $Root "CreamInstaller.sln"
$PublishDir = Join-Path $Root "publish"
$DistDir = Join-Path $Root "dist"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET 8 SDK was not found. Install .NET 8 SDK first."
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) was not found. Install GitHub CLI first."
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$ProjectXml = Get-Content -LiteralPath $Project -Raw
    $VersionNode = $ProjectXml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1
    if ($null -ne $VersionNode) {
        $Version = [string]$VersionNode.Version
    }
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Unable to determine the application version from CreamInstaller.csproj."
}

$Tag = "v$Version"

Write-Host "Repository : $Repo"
Write-Host "Version    : $Version"
Write-Host "Tag        : $Tag"
Write-Host ""

Write-Host "Checking GitHub authentication..."
& gh auth status
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

if (Test-Path -LiteralPath $PublishDir) {
    Remove-Item -LiteralPath $PublishDir -Recurse -Force
}
if (Test-Path -LiteralPath $DistDir) {
    Remove-Item -LiteralPath $DistDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
New-Item -ItemType Directory -Path $DistDir -Force | Out-Null

Write-Host "Restoring dependencies..."
& dotnet restore $Solution
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Host "Publishing Windows x64 single-file build..."
& dotnet publish $Project `
    -c Release `
    -r win-x64 `
    -p:PublishSingleFile=true `
    --self-contained true `
    --output $PublishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

$Exe = Join-Path $PublishDir "CreamInstaller.exe"
if (-not (Test-Path -LiteralPath $Exe)) {
    throw "Build completed without producing CreamInstaller.exe."
}

$ReleaseExe = Join-Path $DistDir "CreamInstaller.exe"
$ReleaseZip = Join-Path $DistDir "CreamInstaller.zip"
Copy-Item -LiteralPath $Exe -Destination $ReleaseExe -Force

Write-Host "Creating CreamInstaller.zip for the built-in updater..."
Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $ReleaseZip -CompressionLevel Optimal -Force

$NotesLines = @(
    "## CreamInstaller Multilingual Build",
    "",
    "- Adds automatic language selection based on the Windows UI language.",
    "- Simplified Chinese is selected automatically for Chinese Windows installations.",
    "- Other Windows UI languages currently fall back to English.",
    "- Adds a language selector in Settings: System Default / English / Simplified Chinese.",
    "- Manual language selection is persisted across application restarts.",
    "- Adds Simplified Chinese translations for common UI, scan, install, update and settings text.",
    "- This fork checks nodesire7/CreamInstaller Releases for updates.",
    "",
    "Assets:",
    "- CreamInstaller.exe: Windows x64 self-contained single-file build.",
    "- CreamInstaller.zip: Package used by the built-in updater."
)
$Notes = [string]::Join([Environment]::NewLine, $NotesLines)

Write-Host "Checking whether Release $Tag already exists..."
# Windows PowerShell 5.1 converts native stderr into ErrorRecord objects.
# gh returns "release not found" on stderr for a missing release, which is an
# expected condition here. Temporarily suppress native-command errors so the
# exit code can be used as the existence test.
$PreviousErrorActionPreference = $ErrorActionPreference
try {
    $ErrorActionPreference = "SilentlyContinue"
    & gh release view $Tag --repo $Repo 1>$null 2>$null
    $ExistingRelease = $LASTEXITCODE -eq 0
}
finally {
    $ErrorActionPreference = $PreviousErrorActionPreference
}

if ($ExistingRelease) {
    Write-Host "Release $Tag already exists; replacing uploaded assets..."
    & gh release upload $Tag $ReleaseExe $ReleaseZip --repo $Repo --clobber
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload Release assets."
    }
} else {
    Write-Host "Creating GitHub Release $Tag..."
    & gh release create $Tag `
        $ReleaseExe `
        $ReleaseZip `
        --repo $Repo `
        --target main `
        --title "CreamInstaller v$Version Multilingual" `
        --notes $Notes
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub Release."
    }
}

Write-Host ""
Write-Host "Release published successfully: https://github.com/$Repo/releases/tag/$Tag"
