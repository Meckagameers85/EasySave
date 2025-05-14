using System.Text.Json;

namespace EasySaveConsole.Models;

public class SettingsManager
{
    public string settingsFile = "settings.json";
    public string currentLanguage { get; private set; } = "en";

    public SettingsManager()
    {
        LoadSettings();
    }

    public string ChangeLanguage(string newLanguageCode)
    {
        currentLanguage = newLanguageCode;
        SaveSettings();
        return $"Langue définie sur : {currentLanguage}";
    }

    private void SaveSettings()
    {
        var settings = new SettingsData { currentLanguage = this.currentLanguage };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsFile, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(settingsFile))
        {
            try
            {
                var json = File.ReadAllText(settingsFile);
                var loaded = JsonSerializer.Deserialize<SettingsData>(json);
                if (loaded is not null)
                    currentLanguage = loaded.currentLanguage;
            }
            catch
            {
                currentLanguage = "en"; // Valeur par défaut de secours
            }
        }
    }

    private class SettingsData
    {
        public string currentLanguage { get; set; } = "en";
    }
}
