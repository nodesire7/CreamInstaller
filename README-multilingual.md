# CreamInstaller 多语言版说明

此 fork 增加了应用内多语言支持。

## 当前语言

- 跟随系统：根据 Windows UI 语言自动选择。中文系统使用简体中文，其他语言回退到 English。
- English
- 简体中文

语言可以在 **Settings / 设置 → Language / 语言** 中手动修改，并保存到现有的 `settings.json`。

## 不使用 GitHub Actions 的 Release 发布

在 Windows 上安装 .NET 8 SDK 与 GitHub CLI，并完成 `gh auth login` 后，在仓库根目录执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\Build-Release.ps1
```

脚本会直接构建 Windows x64 自包含单文件版本，并上传以下资产到 `nodesire7/CreamInstaller` 的 GitHub Releases：

- `CreamInstaller.exe`
- `CreamInstaller.zip`（程序内置更新器使用）

此发布方式不依赖 GitHub Actions。
