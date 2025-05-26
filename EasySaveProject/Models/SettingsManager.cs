using System.Text.Json;
using LoggerLib;

namespace EasySaveProject.Models;

public class SettingsManager
{
    
    private static readonly Lazy<SettingsManager> _instance = new(() => new SettingsManager());
    public static SettingsManager instance => _instance.Value;
    public string settingsFile = "settings.json";
    public string currentLanguage { get; private set; } = "en";

    public string formatLogger { get; private set; } = "JSON";

    private SettingsManager()
    {
        /* 
            Visibility : public
            Input : None
            Output : None
            Description : Constructor of the SettingsManager class. It loads the settings from the JSON file.
        */
        LoadSettings();
    }

    public void ChangeLanguage(string newLanguageCode)
    {
        /* 
            Visibility : public
            Input : string newLanguageCode
            Output : None
            Description : Changes the current language to the provided language code and saves the settings into a JSON file.
        */
        currentLanguage = newLanguageCode;
        SaveSettings();
    }

    public void ChangeFormatLogger(string newFormatLogger)
    {
        /* 
            Visibility : public
            Input : string newFormatLogger
            Output : None
            Description : Changes the current format logger to the provided format logger and saves the settings into a JSON file.
        */
        formatLogger = newFormatLogger;
        SaveSettings();
    }

    private void SaveSettings()
    {
        /* 
            Visibility : private
            Input : None
            Output : None
            Description : Saves the current settings into a JSON file.
        */
        var settings = new SettingsData { currentLanguage = this.currentLanguage, formatLogger = this.formatLogger };
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(settingsFile, json);
    }

    private void LoadSettings()
    {
        /* 
            Visibility : private
            Input : None
            Output : None
            Description : Loads the settings from a JSON file. If the file does not exist or is invalid, sets the currentLanguage to a default value.
        */
        if (File.Exists(settingsFile))
        {
            try
            {
                var json = File.ReadAllText(settingsFile);
                var loaded = JsonSerializer.Deserialize<SettingsData>(json);
                if (loaded is not null)
                {
                    currentLanguage = loaded.currentLanguage;
                    formatLogger = loaded.formatLogger;
                }
            }
            catch
            {
                currentLanguage = "en"; // Default value
                formatLogger = "JSON"; // Default value
            }
        }
    }

    private class SettingsData
    {
        public string currentLanguage { get; set; } = "en";
        public string formatLogger { get; set; } = "JSON";
    }
}
