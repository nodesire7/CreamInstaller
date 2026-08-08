using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace CreamInstaller.Utility;

internal static class LocalizationManager
{
    internal const string Auto = "auto";
    internal const string English = "en";
    internal const string SimplifiedChinese = "zh-CN";

    private sealed class ControlState
    {
        internal string SourceText = "";
        internal string SourcePlaceholder = "";
        internal bool Applying;
        internal bool Hooked;
    }

    private static readonly ConditionalWeakTable<Control, ControlState> States = new();

    private static readonly Dictionary<string, string> ZhCn = new(StringComparer.Ordinal)
    {
        ["Settings"] = "设置",
        ["Appearance"] = "外观",
        ["Enable Dark Mode"] = "启用深色模式",
        ["Language"] = "语言",
        ["System Default"] = "跟随系统",
        ["English"] = "English",
        ["Simplified Chinese"] = "简体中文",
        ["Game Management"] = "游戏管理",
        ["Block Protected Games"] = "屏蔽受保护的游戏",
        ["Sort game list by name"] = "按名称排序游戏列表",
        ["Maintenance"] = "维护",
        ["Clear Cached Data"] = "清除缓存数据",
        ["Reconfigure SteamCMD"] = "重新配置 SteamCMD",
        ["Save"] = "保存",
        ["Cancel"] = "取消",
        ["Programs & Games"] = "程序和游戏",
        ["No applicable programs and/or games found."] = "未找到可用的程序或游戏。",
        ["Selected Unlocker: SmokeAPI"] = "当前解锁器：SmokeAPI",
        ["Selected Unlocker: CreamAPI"] = "当前解锁器：CreamAPI",
        ["Select All"] = "全选",
        ["All"] = "全部",
        ["Generate and Install"] = "生成并安装",
        ["Rescan"] = "重新扫描",
        ["Uninstall Selected"] = "卸载所选项目",
        ["Gathering and caching programs . . ."] = "正在收集并缓存程序……",
        ["Waiting for user to select which programs/games to scan . . ."] = "等待选择要扫描的程序 / 游戏……",
        ["Loading previously installed DLC unlockers from last session..."] = "正在加载上次会话中已安装的 DLC 解锁器……",
        ["Setting up SteamCMD . . . "] = "正在设置 SteamCMD……",
        ["Gathering and caching your applicable games and their DLCs . . . "] = "正在收集并缓存可用游戏及其 DLC……",
        ["Choose which programs and/or games to scan:"] = "选择要扫描的程序和 / 或游戏：",
        ["Choices"] = "选择",
        ["Sort By Name"] = "按名称排序",
        ["Load"] = "加载",
        ["OK"] = "确定",
        ["Retry"] = "重试",
        ["Loading . . . "] = "正在加载……",
        ["Reselect Programs / Games"] = "重新选择程序 / 游戏",
        ["Checking for updates . . ."] = "正在检查更新……",
        ["Update"] = "更新",
        ["Ignore"] = "忽略",
        ["Updating . . . "] = "正在更新……",
        ["Platform"] = "平台",
        ["App ID:"] = "应用 ID：",
        ["Game Name:"] = "游戏名称：",
        ["Search"] = "搜索",
        ["Generate Test Game"] = "生成测试游戏",
        ["Clear All Tests"] = "清除全部测试",
        ["Close"] = "关闭",
        ["Clear Cache"] = "清除缓存",
        ["Reconfigure"] = "重新配置",
        ["Reconfiguring..."] = "正在重新配置……",
        ["Complete"] = "完成",
        ["Repairing Paradox Launcher . . . "] = "正在修复 Paradox Launcher……",
        ["The operation was canceled."] = "操作已取消。",
        ["Enter the name of a game to search"] = "输入游戏名称进行搜索",
        ["e.g. 480"] = "例如：480",
        ["This will delete all cached game data, installed game records, and proxy configurations.\n\nYour settings will be preserved. A fresh scan will be required on the next launch."] = "这将删除所有缓存的游戏数据、已安装游戏记录和代理配置。\n\n你的设置会被保留。下次启动时需要重新扫描。",
        ["This will delete and re-download the SteamCMD installation.\n\nCached app data will be preserved."] = "这将删除并重新下载 SteamCMD。\n\n已缓存的应用数据会被保留。",
        ["[Experimental] WARNING: This may still be unstable.\nThis setting restores the use of SmokeAPI.\nIf some games don't launch with SmokeAPI enabled, try disabling this setting then Generate and Install again."] = "[实验性] 警告：此功能可能仍不稳定。\n此设置会重新启用 SmokeAPI。\n如果某些游戏在启用 SmokeAPI 后无法启动，请关闭此设置，然后再次执行“生成并安装”。"
    };

    internal static string ConfiguredLanguage { get; private set; } = Auto;
    internal static string EffectiveLanguage { get; private set; } = English;

    internal static void Initialize(string configuredLanguage)
    {
        ConfiguredLanguage = NormalizeConfiguredLanguage(configuredLanguage);
        EffectiveLanguage = ResolveEffectiveLanguage(ConfiguredLanguage);
        ApplyCulture();
    }

    internal static void SetLanguage(string configuredLanguage)
    {
        Initialize(configuredLanguage);
        ApplyToAllOpenForms();
    }

    internal static string NormalizeConfiguredLanguage(string language)
        => language switch
        {
            English => English,
            SimplifiedChinese => SimplifiedChinese,
            _ => Auto
        };

    private static string ResolveEffectiveLanguage(string configuredLanguage)
    {
        if (configuredLanguage != Auto)
            return configuredLanguage;

        // InstalledUICulture reflects the Windows UI language and is not changed
        // when the user manually switches the application language at runtime.
        string name = CultureInfo.InstalledUICulture.Name;
        return name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? SimplifiedChinese
            : English;
    }

    private static void ApplyCulture()
    {
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(EffectiveLanguage);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch
        {
            // Keep the process culture if a requested culture is unavailable.
        }
    }

    internal static string GetLanguageDisplayName(string language)
        => language switch
        {
            Auto => Translate("System Default"),
            SimplifiedChinese => "简体中文",
            _ => "English"
        };

    internal static string Translate(string text)
    {
        if (string.IsNullOrEmpty(text) || EffectiveLanguage == English)
            return text;

        if (ZhCn.TryGetValue(text, out string translated))
            return translated;

        if (text.IndexOfAny(['\r', '\n']) >= 0)
            return TranslateMultiline(text);

        return TranslateSingleLine(text);
    }

    private static string TranslateMultiline(string text)
    {
        StringBuilder result = new(text.Length + 64);
        int lineStart = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c is not ('\r' or '\n'))
                continue;

            result.Append(TranslateSingleLine(text[lineStart..i]));

            if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                result.Append("\r\n");
                i++;
            }
            else
                result.Append(c);

            lineStart = i + 1;
        }

        if (lineStart < text.Length)
            result.Append(TranslateSingleLine(text[lineStart..]));

        return result.ToString();
    }

    private static string TranslateSingleLine(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace('\u00A0', ' ');

        if (ZhCn.TryGetValue(text, out string translated))
            return translated;

        if (text.StartsWith("An update is available: v", StringComparison.Ordinal))
            return "发现可用更新：v" + text[25..];

        if (text.StartsWith("Updating . . . ", StringComparison.Ordinal))
            return "正在更新…… " + text[15..];

        if (text.StartsWith("Setting up SteamCMD . . . ", StringComparison.Ordinal))
            return "正在设置 SteamCMD…… " + text[26..];

        const string gathering = "Gathering and caching your applicable games and their DLCs . . . ";
        if (text.StartsWith(gathering, StringComparison.Ordinal))
            return "正在收集并缓存可用游戏及其 DLC…… " + text[gathering.Length..];

        const string loadingCache = "Loading games and DLCs from cached data... ";
        if (text.StartsWith(loadingCache, StringComparison.Ordinal))
            return "正在从缓存加载游戏和 DLC…… " + text[loadingCache.Length..];

        if (text.StartsWith("Remaining games (", StringComparison.Ordinal))
            return "剩余游戏（" + text[17..].Replace("): ", "）：", StringComparison.Ordinal);

        if (text.StartsWith("Remaining DLCs (", StringComparison.Ordinal))
            return "剩余 DLC（" + text[16..].Replace("): ", "）：", StringComparison.Ordinal);

        if (text.StartsWith("Found ", StringComparison.Ordinal) && text.EndsWith(" games", StringComparison.Ordinal))
            return "已找到 " + text[6..^6] + " 个游戏";

        if (text.StartsWith("Found ", StringComparison.Ordinal) && text.EndsWith(" DLCs", StringComparison.Ordinal))
            return "已找到 " + text[6..^5] + " 个 DLC";

        if (text.StartsWith("Games: ", StringComparison.Ordinal))
            return "游戏：" + text[7..];

        if (text.StartsWith("DLCs: ", StringComparison.Ordinal))
            return "DLC：" + text[6..];

        if (text.StartsWith("Operation succeeded for ", StringComparison.Ordinal))
            return ChineseSentence("操作成功：" + text[24..]);

        if (text.StartsWith("Operation failed for ", StringComparison.Ordinal))
            return ChineseSentence("操作失败：" + text[21..]);

        const string installSummary = "DLC unlocker(s) successfully installed and generated for ";
        if (text.StartsWith(installSummary, StringComparison.Ordinal) && text.EndsWith(" program(s).", StringComparison.Ordinal))
            return "已成功为 " + text[installSummary.Length..^12] + " 个程序安装并生成 DLC 解锁器。";

        const string uninstallSummary = "DLC unlocker(s) successfully uninstalled for ";
        if (text.StartsWith(uninstallSummary, StringComparison.Ordinal) && text.EndsWith(" program(s).", StringComparison.Ordinal))
            return "已成功为 " + text[uninstallSummary.Length..^12] + " 个程序卸载 DLC 解锁器。";

        const string installFailure = "DLC unlocker installation and/or generation failed: ";
        if (text.StartsWith(installFailure, StringComparison.Ordinal))
            return "DLC 解锁器安装和/或生成失败：" + text[installFailure.Length..];

        const string uninstallFailure = "DLC unlocker uninstallation failed: ";
        if (text.StartsWith(uninstallFailure, StringComparison.Ordinal))
            return "DLC 解锁器卸载失败：" + text[uninstallFailure.Length..];

        if (TryTranslatePrefixed(text, "Wrote 32-bit SmokeAPI: ", "已写入 32 位 SmokeAPI：", out translated)
            || TryTranslatePrefixed(text, "Wrote 64-bit SmokeAPI: ", "已写入 64 位 SmokeAPI：", out translated)
            || TryTranslatePrefixed(text, "Wrote SmokeAPI: ", "已写入 SmokeAPI：", out translated)
            || TryTranslatePrefixed(text, "Wrote 32-bit CreamAPI: ", "已写入 32 位 CreamAPI：", out translated)
            || TryTranslatePrefixed(text, "Wrote 64-bit CreamAPI: ", "已写入 64 位 CreamAPI：", out translated)
            || TryTranslatePrefixed(text, "Wrote CreamAPI: ", "已写入 CreamAPI：", out translated)
            || TryTranslatePrefixed(text, "Deleted old configuration: ", "已删除旧配置：", out translated)
            || TryTranslatePrefixed(text, "Deleted unnecessary configuration: ", "已删除不必要的配置：", out translated)
            || TryTranslatePrefixed(text, "Deleted configuration: ", "已删除配置：", out translated)
            || TryTranslatePrefixed(text, "Deleted cache: ", "已删除缓存：", out translated)
            || TryTranslatePrefixed(text, "Deleted log: ", "已删除日志：", out translated)
            || TryTranslatePrefixed(text, "Deleted SmokeAPI: ", "已删除 SmokeAPI：", out translated)
            || TryTranslatePrefixed(text, "Deleted CreamAPI: ", "已删除 CreamAPI：", out translated)
            || TryTranslatePrefixed(text, "Renamed Steamworks: ", "已重命名 Steamworks：", out translated)
            || TryTranslatePrefixed(text, "Restored Steamworks: ", "已还原 Steamworks：", out translated))
            return translated;

        if (text.StartsWith("Added locked DLC to SmokeAPI.config.json with appid ", StringComparison.Ordinal))
            return "已向 SmokeAPI.config.json 添加锁定 DLC，AppID " + text[50..];

        if (text.StartsWith("Added extra DLC to SmokeAPI.config.json with appid ", StringComparison.Ordinal))
            return "已向 SmokeAPI.config.json 添加额外 DLC，AppID " + text[49..];

        if (text.StartsWith("Installing ", StringComparison.Ordinal))
            return TranslateOperation(text, false);

        if (text.StartsWith("Uninstalling ", StringComparison.Ordinal))
            return TranslateOperation(text, true);

        return text;
    }

    private static bool TryTranslatePrefixed(string text, string prefix, string translatedPrefix, out string translated)
    {
        if (text.StartsWith(prefix, StringComparison.Ordinal))
        {
            translated = translatedPrefix + text[prefix.Length..];
            return true;
        }

        translated = "";
        return false;
    }

    private static string ChineseSentence(string text)
        => text.EndsWith('.', StringComparison.Ordinal) ? text[..^1] + "。" : text;

    private static string TranslateOperation(string text, bool uninstalling)
    {
        string prefix = uninstalling ? "Uninstalling " : "Installing ";
        string result = (uninstalling ? "正在卸载 " : "正在安装 ") + text[prefix.Length..];
        result = result.Replace(" in incorrect directory ", "，错误目录：", StringComparison.Ordinal);
        result = result.Replace(" in directory ", "，目录：", StringComparison.Ordinal);
        result = result.Replace(" with root directory ", "，根目录：", StringComparison.Ordinal);
        result = result.Replace(" in proxy mode from ", "（代理模式），来源：", StringComparison.Ordinal);
        result = result.Replace(" in proxy mode for ", "（代理模式），目标：", StringComparison.Ordinal);
        result = result.Replace(" from ", "，来源：", StringComparison.Ordinal);
        result = result.Replace(" for ", "，目标：", StringComparison.Ordinal);
        result = result.Replace(" . . .", "……", StringComparison.Ordinal);
        return result.TrimEnd();
    }

    internal static void ApplyToAllOpenForms()
    {
        foreach (Form form in Application.OpenForms)
            Apply(form);
    }

    internal static void Apply(Form form)
    {
        ApplyControl(form);
        foreach (Control control in form.Controls)
            ApplyControlTree(control);
    }

    private static void ApplyControlTree(Control control)
    {
        ApplyControl(control);
        foreach (Control child in control.Controls)
            ApplyControlTree(child);
    }

    private static void ApplyControl(Control control)
    {
        ControlState state = States.GetOrCreateValue(control);
        if (!state.Hooked)
        {
            state.SourceText = control.Text ?? "";
            if (control is TextBox textBox)
                state.SourcePlaceholder = textBox.PlaceholderText ?? "";

            control.TextChanged += (_, _) =>
            {
                if (state.Applying)
                    return;
                state.SourceText = control.Text ?? "";
                ApplyControlText(control, state);
            };
            state.Hooked = true;
        }

        ApplyControlText(control, state);

        if (control is TextBox box)
        {
            if (string.IsNullOrEmpty(state.SourcePlaceholder))
                state.SourcePlaceholder = box.PlaceholderText ?? "";
            box.PlaceholderText = EffectiveLanguage == English
                ? state.SourcePlaceholder
                : Translate(state.SourcePlaceholder);
        }
    }

    private static void ApplyControlText(Control control, ControlState state)
    {
        string target = EffectiveLanguage == English ? state.SourceText : Translate(state.SourceText);
        if (control.Text == target)
            return;
        state.Applying = true;
        try
        {
            control.Text = target;
        }
        finally
        {
            state.Applying = false;
        }
    }
}
