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
    $Version = [string]($ProjectXml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
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
gh auth status
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run: gh auth login"
}

if (Test-Path -LiteralPath $PublishDir) {
    Remove-Item -LiteralPath $PublishDir -Recurse -Force
}
if (Test-Path -LiteralPath $DistDir) {
    Remove-Item -LiteralPath $DistDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Path $DistDir | Out-Null

Write-Host "Restoring dependencies..."
dotnet restore $Solution
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Host "Publishing Windows x64 single-file build..."
dotnet publish $Project `
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

$Notes = @"
## CreamInstaller 多语言版

- 新增根据 Windows 系统 UI 语言自动切换语言。
- 中文 Windows 默认使用简体中文，其他系统默认使用 English。
- 设置中新增语言选择：跟随系统 / English / 简体中文。
- 手动语言选择会保存并在下次启动继续使用。
- 主界面、扫描、安装、更新、设置及常用提示加入简体中文翻译。
- 更新源指向 nodesire7/CreamInstaller Releases。

本 Release 同时提供：
- CreamInstaller.exe：Windows x64 自包含单文件版本。
- CreamInstaller.zip：程序内置自动更新功能使用的更新包。
"@

Write-Host "Checking whether Release $Tag already exists..."
gh release view $Tag --repo $Repo *> $null
$ExistingRelease = $LASTEXITCODE -eq 0

if ($ExistingRelease) {
    Write-Host "Release $Tag already exists; replacing uploaded assets..."
    gh release upload $Tag $ReleaseExe $ReleaseZip --repo $Repo --clobber
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload Release assets."
    }
} else {
    Write-Host "Creating GitHub Release $Tag..."
    gh release create $Tag `
        $ReleaseExe `
        $ReleaseZip `
        --repo $Repo `
        --target main `
        --title "CreamInstaller v$Version 多语言版" `
        --notes $Notes
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub Release."
    }
}

Write-Host ""
Write-Host "Release published successfully: https://github.com/$Repo/releases/tag/$Tag"
