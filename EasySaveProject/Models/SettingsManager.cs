using System.Text.Json;

namespace EasySaveProject.Models;

public class SettingsManager
{
    public string settingsFile = "settings.json";
    public string currentLanguage { get; private set; } = "en";

    public SettingsManager()
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

    private void SaveSettings()
    {
        /* 
            Visibility : private
            Input : None
            Output : None
            Description : Saves the current settings into a JSON file.
        */
        var settings = new SettingsData { currentLanguage = this.currentLanguage };
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
                    currentLanguage = loaded.currentLanguage;
            }
            catch
            {
                currentLanguage = "en"; // Default value
            }
        }
    }

    private class SettingsData
    {
        public string currentLanguage { get; set; } = "en";
    }
}
