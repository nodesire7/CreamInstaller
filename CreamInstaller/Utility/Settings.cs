namespace CreamInstaller.Utility;

internal sealed class SettingsModel
{
    private string language = LocalizationManager.Auto;

    public SettingsModel()
    {
        LocalizationManager.Initialize(language);
    }

    public bool UseSmokeAPI { get; set; } = true;
    public bool BlockProtectedGames { get; set; } = true;
    public bool DarkModeEnabled { get; set; } = true;
    public bool SortByName { get; set; } = true;

    public string Language
    {
        get => language;
        set
        {
            language = LocalizationManager.NormalizeConfiguredLanguage(value);
            LocalizationManager.Initialize(language);
        }
    }
}
