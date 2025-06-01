using System.ComponentModel;
using System.Windows.Input;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class SettingsCryptosoftWindowViewModel : INotifyPropertyChanged
{
    private readonly CryptoSoftManager _cryptoSoftManager;
    public event Action? RequestClose;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CloseCommand { get; }

    public SettingsCryptosoftWindowViewModel()
    {
        _cryptoSoftManager = CryptoSoftManager.instance;
        _allowedExtensionsText = string.Join(", ", _cryptoSoftManager.Settings.extensions);
        SaveCommand = new RelayCommand(() =>
        {
            SaveSettings();
            CloseWindow();
        });
        ResetCommand = new RelayCommand(ResetSettings);
        CloseCommand = new RelayCommand(CloseWindow);
    }
    private string _allowedExtensionsText;
    public string AllowedExtensions
    {
        get => _allowedExtensionsText;
        set
        {
            _allowedExtensionsText = value;
            OnPropertyChanged();

            // Traitement réel
            _cryptoSoftManager.Settings.extensions = value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim())
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .ToList();
        }
    }

    public string PublicKeyPath
    {
        get => _cryptoSoftManager.Settings.PublicKey;
        set
        {
            _cryptoSoftManager.Settings.PublicKey = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string PrivateKeyPath
    {
        get => _cryptoSoftManager.Settings.PrivateKey;
        set
        {
            _cryptoSoftManager.Settings.PrivateKey = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    private void SaveSettings()
    {
        if (_cryptoSoftManager.Settings.extensions is { Count: > 0 })
            _cryptoSoftManager.SetExtensions(_cryptoSoftManager.Settings.extensions);
        if (!string.IsNullOrWhiteSpace(PublicKeyPath))
            _cryptoSoftManager.SetPublicKeyRSA(PublicKeyPath);

        if (!string.IsNullOrWhiteSpace(PrivateKeyPath))
            _cryptoSoftManager.SetPrivateKeyRSA(PrivateKeyPath);
    }

    private void ResetSettings()
    {
        AllowedExtensions = string.Empty;
        PublicKeyPath = string.Empty;
        PrivateKeyPath = string.Empty;
        SaveSettings();
    }
    private void CloseWindow() => RequestClose?.Invoke();

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}