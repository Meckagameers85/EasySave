using System.Text.Json;
using LoggerLib;
using System.IO;

namespace EasySaveProject.Models;

public class SettingsManager
{
    private static readonly Lazy<SettingsManager> _instance = new(() => new SettingsManager());
    public static SettingsManager instance => _instance.Value;
    public string settingsFile = "settings.json";
    public string currentLanguage { get; private set; } = "en";
    public string formatLogger { get; private set; } = "JSON";
    public string businessSoftwareName { get; private set; } = "";

    // NOUVEAU : Seuil pour la gestion de bande passante (en MB)
    public double bandwidthThresholdMB { get; private set; } = 10.0; // Défaut: 10 MB

    private SettingsManager()
    {
        LoadSettings();
    }

    public void ChangeLanguage(string newLanguageCode)
    {
        currentLanguage = newLanguageCode;
        SaveSettings();
    }

    public void ChangeFormatLogger(string newFormatLogger)
    {
        formatLogger = newFormatLogger;
        SaveSettings();
    }

    public void SetBusinessSoftwareName(string softwareName)
    {
        businessSoftwareName = softwareName?.Trim() ?? "";
        SaveSettings();
    }

    // NOUVELLE MÉTHODE : Définir le seuil de bande passante
    public void SetBandwidthThreshold(double thresholdMB)
    {
        // Valider la valeur (entre 1 MB et 1000 MB)
        if (thresholdMB < 1.0) thresholdMB = 1.0;
        if (thresholdMB > 1000.0) thresholdMB = 1000.0;

        bandwidthThresholdMB = thresholdMB;
        SaveSettings();
    }

    // MÉTHODE UTILITAIRE : Convertir en bytes pour comparaison
    public long GetBandwidthThresholdBytes()
    {
        return (long)(bandwidthThresholdMB * 1024 * 1024); // MB vers bytes
    }

    private void SaveSettings()
    {
        var settings = new SettingsData
        {
            currentLanguage = this.currentLanguage,
            formatLogger = this.formatLogger,
            businessSoftwareName = this.businessSoftwareName,
            bandwidthThresholdMB = this.bandwidthThresholdMB // NOUVEAU
        };
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
                {
                    currentLanguage = loaded.currentLanguage;
                    formatLogger = loaded.formatLogger;
                    businessSoftwareName = loaded.businessSoftwareName ?? "";
                    bandwidthThresholdMB = loaded.bandwidthThresholdMB; // NOUVEAU
                }
            }
            catch
            {
                // Valeurs par défaut
                currentLanguage = "en";
                formatLogger = "JSON";
                businessSoftwareName = "calc";
                bandwidthThresholdMB = 10.0; // NOUVEAU : 10 MB par défaut
            }
        }
    }

    private class SettingsData
    {
        public string currentLanguage { get; set; } = "en";
        public string formatLogger { get; set; } = "JSON";
        public string businessSoftwareName { get; set; } = "";
        public double bandwidthThresholdMB { get; set; } = 10.0; // NOUVEAU
    }
}