using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
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
    private static bool idleHooked;

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
        ["Enter the name of a game to search"] = "输入游戏名称进行搜索",
        ["e.g. 480"] = "例如：480",
        ["This will delete all cached game data, installed game records, and proxy configurations.\n\nYour settings will be preserved. A fresh scan will be required on the next launch."] = "这将删除所有缓存的游戏数据、已安装游戏记录和代理配置。\n\n你的设置会被保留。下次启动时需要重新扫描。",
        ["This will delete and re-download the SteamCMD installation.\n\nCached app data will be preserved."] = "这将删除并重新下载 SteamCMD。\n\n已缓存的应用数据会被保留。"
    };

    internal static string ConfiguredLanguage { get; private set; } = Auto;
    internal static string EffectiveLanguage { get; private set; } = English;

    internal static void Initialize(string configuredLanguage)
    {
        ConfiguredLanguage = NormalizeConfiguredLanguage(configuredLanguage);
        EffectiveLanguage = ResolveEffectiveLanguage(ConfiguredLanguage);
        ApplyCulture();
        HookGlobalRefresh();
    }

    internal static void SetLanguage(string configuredLanguage)
    {
        Initialize(configuredLanguage);
        ApplyToAllOpenForms();
    }

    private static void HookGlobalRefresh()
    {
        if (idleHooked)
            return;
        Application.Idle += (_, _) => ApplyToAllOpenForms();
        idleHooked = true;
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

        if (text.StartsWith("Found ", StringComparison.Ordinal) && text.EndsWith(" games", StringComparison.Ordinal))
            return "已找到 " + text[6..^6] + " 个游戏";
        if (text.StartsWith("Found ", StringComparison.Ordinal) && text.EndsWith(" DLCs", StringComparison.Ordinal))
            return "已找到 " + text[6..^5] + " 个 DLC";
        if (text.StartsWith("Games: ", StringComparison.Ordinal))
            return "游戏：" + text[7..];
        if (text.StartsWith("DLCs: ", StringComparison.Ordinal))
            return "DLC：" + text[6..];

        return text;
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
