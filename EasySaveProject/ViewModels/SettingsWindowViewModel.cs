using System.ComponentModel;
using System.Windows.Input;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly SettingsManager _settingsManager;
    private readonly LanguageManager _languageManager;
    public event Action? RequestClose;
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand TestBusinessSoftwareCommand { get; }
    public event Action<string>? BusinessSoftwareTested;
    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsWindowViewModel()
    {
        _settingsManager = SettingsManager.instance;
        _languageManager = LanguageManager.instance;

        SelectedLanguageCode = _settingsManager.currentLanguage;
        SelectedLogFormat = _settingsManager.formatLogger;
        BusinessSoftwareName = _settingsManager.businessSoftwareName;
        BandwidthThresholdMB = _settingsManager.bandwidthThresholdMB; // NOUVEAU

        SaveCommand = new RelayCommand(SaveSettings);
        ResetCommand = new RelayCommand(ResetSettings);
        CloseCommand = new RelayCommand(CloseWindow);
        TestBusinessSoftwareCommand = new RelayCommand(TestBusinessSoftware);
    }

    #region Labels
    public string WindowTitle => _languageManager.Translate("settings.title");
    public string SaveButtonText => _languageManager.Translate("Settings.SaveButtonText");
    public string ResetButtonText => _languageManager.Translate("Settings.ResetButtonText");
    public string LanguageLabel => _languageManager.Translate("Settings.LanguageLabel");
    public string FormatLabel => _languageManager.Translate("Settings.FormatLabel");
    public string BusinessSoftwareNameLabel => _languageManager.Translate("Settings.businessSoftwareNameLabel");
    public string BandwidthThresholdLabel => "Seuil fichiers volumineux (MB):"; // NOUVEAU
    public string EnglishOption => _languageManager.Translate("lang.en");
    public string FrenchOption => _languageManager.Translate("lang.fr");
    public string SpanishOption => _languageManager.Translate("lang.es");
    public string GermanOption => _languageManager.Translate("lang.de");
    public string RussianOption => _languageManager.Translate("lang.ru");
    public string ItalianOption => _languageManager.Translate("lang.it");
    public string PortugueseOption => _languageManager.Translate("lang.pt");
    #endregion

    private string _languageCode = "en";
    public string SelectedLanguageCode
    {
        get => _languageCode;
        set
        {
            if (_languageCode != value)
            {
                _languageCode = value;
                OnPropertyChanged();
            }
        }
    }

    private string _logFormat = "JSON";
    public string SelectedLogFormat
    {
        get => _logFormat;
        set
        {
            if (_logFormat != value)
            {
                _logFormat = value;
                OnPropertyChanged();
            }
        }
    }

    private string _businessSoftwareName = "";
    public string BusinessSoftwareName
    {
        get => _businessSoftwareName;
        set
        {
            if (_businessSoftwareName != value)
            {
                _businessSoftwareName = value;
                OnPropertyChanged();
            }
        }
    }

    // NOUVELLE PROPRIÉTÉ : Seuil de bande passante
    private double _bandwidthThresholdMB = 10.0;
    public double BandwidthThresholdMB
    {
        get => _bandwidthThresholdMB;
        set
        {
            // Valider la valeur
            if (value < 1.0) value = 1.0;
            if (value > 1000.0) value = 1000.0;

            if (Math.Abs(_bandwidthThresholdMB - value) > 0.01)
            {
                _bandwidthThresholdMB = value;
                OnPropertyChanged();
            }
        }
    }

    private void SetSettings()
    {
        _settingsManager.SetBusinessSoftwareName(_businessSoftwareName);
        _settingsManager.ChangeLanguage(SelectedLanguageCode);
        _settingsManager.ChangeFormatLogger(SelectedLogFormat);
        _settingsManager.SetBandwidthThreshold(_bandwidthThresholdMB); // NOUVEAU
        _languageManager.Load(SelectedLanguageCode);
    }

    private void SaveSettings()
    {
        SetSettings();
        CloseWindow();
    }

    private void ResetSettings()
    {
        BusinessSoftwareName = "";
        SelectedLanguageCode = "en";
        SelectedLogFormat = "JSON";
        BandwidthThresholdMB = 10.0; // NOUVEAU : Reset à 10 MB
        SetSettings();
        ReloadWindow();
    }

    private void TestBusinessSoftware()
    {
        var processMonitor = new ProcessMonitor(BusinessSoftwareName);
        if (processMonitor.IsBusinessSoftwareRunning())
            BusinessSoftwareTested?.Invoke("42");
        else
            BusinessSoftwareTested?.Invoke(string.Empty);
    }

    private void ReloadWindow()
    {
        OnPropertyChanged(nameof(SelectedLanguageCode));
        OnPropertyChanged(nameof(SelectedLogFormat));
        OnPropertyChanged(nameof(BandwidthThresholdMB)); // NOUVEAU
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(ResetButtonText));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(FormatLabel));
        OnPropertyChanged(nameof(BusinessSoftwareNameLabel));
        OnPropertyChanged(nameof(BandwidthThresholdLabel)); // NOUVEAU
        OnPropertyChanged(nameof(EnglishOption));
        OnPropertyChanged(nameof(FrenchOption));
        OnPropertyChanged(nameof(SpanishOption));
        OnPropertyChanged(nameof(GermanOption));
        OnPropertyChanged(nameof(RussianOption));
        OnPropertyChanged(nameof(ItalianOption));
        OnPropertyChanged(nameof(PortugueseOption));
    }

    private void CloseWindow() => RequestClose?.Invoke();

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}