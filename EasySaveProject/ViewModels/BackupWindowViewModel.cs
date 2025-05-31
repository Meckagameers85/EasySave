using System.ComponentModel;
using System.Windows.Input;
using System.IO;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class BackupWindowViewModel : INotifyPropertyChanged
{
    private readonly BackupManager _backupManager;
    private readonly LanguageManager _languageManager;
    private bool IsNewBackup;
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public event Action? RequestClose;
    public event Action<List<string>>? HighlightErrors;
    public event PropertyChangedEventHandler? PropertyChanged;


    public BackupWindowViewModel()
    {
        IsNewBackup = true; // Par défaut, on considère que c'est une nouvelle sauvegarde
        _backupManager = BackupManager.instance;
        _languageManager = LanguageManager.instance;
        SaveCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
    }

    public BackupWindowViewModel(string backupName, string backupSourcePath, string backupTargetPath, string backupType, bool isEncrypted)
    {
        IsNewBackup = false; // On est en mode édition
        _backupManager = BackupManager.instance;
        _languageManager = LanguageManager.instance;
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        BackupName = backupName;
        BackupSource = backupSourcePath;
        BackupDestination = backupTargetPath;
        BackupType = backupType;
        IsBackupEncrypted = isEncrypted;
        _oldBackupName = backupName;
    }


    public string WindowTitle => IsNewBackup  ? _languageManager.Translate("Backup.NewWindowTitle") : _languageManager.Translate("Backup.EditWindowTitle");
    public string SaveButtonText => IsNewBackup  ? _languageManager.Translate("Backup.CreateButtonText") : _languageManager.Translate("Backup.SaveButtonText");
    public string CancelButtonText => _languageManager.Translate("Backup.CancelButtonText");
    public string NameLabel => _languageManager.Translate("Backup.NameLabel");
    public string SourcePathLabel => _languageManager.Translate("Backup.SourcePathLabel");
    public string DestinationPathLabel => _languageManager.Translate("Backup.DestinationPathLabel");
    public string TypeLabel => _languageManager.Translate("Backup.TypeLabel");
    public string FullBackupOption => _languageManager.Translate("Backup.FullBackupOption");
    public string DifferentialBackupOption => _languageManager.Translate("Backup.DifferentialBackupOption");
    public string EncryptCheckBoxLabel => _languageManager.Translate("Backup.EncryptLabel");

    private string _oldBackupName = string.Empty;
    private string _backupName = string.Empty;
    public string BackupName
    {
        get => _backupName;
        set
        {
            if (_backupName != value)
            {
                _backupName = value;
                OnPropertyChanged();
            }
        }
    }

    private string _backupSource = string.Empty;
    public string BackupSource
    {
        get => _backupSource;
        set
        {
            if (_backupSource != value)
            {
                _backupSource = value;
                OnPropertyChanged();
            }
        }
    }

    private string _backupDestination = string.Empty;
    public string BackupDestination
    {
        get => _backupDestination;
        set
        {
            if (_backupDestination != value)
            {
                _backupDestination = value;
                OnPropertyChanged();
            }
        }
    }
    private SaveType _backupType = SaveType.Full;
    public string BackupType
    {
        get => _backupType.ToString();
        set
        {
            if (Enum.TryParse<SaveType>(value, true, out var parsed) && _backupType != parsed)
            {
                _backupType = parsed;
                OnPropertyChanged();
            }
        }
    }

    private bool _isBackupEncrypted = false;
    public bool IsBackupEncrypted
    {
        get => _isBackupEncrypted;
        set
        {
            if (_isBackupEncrypted != value)
            {
                _isBackupEncrypted = value;
                OnPropertyChanged();
            }
        }
    }

    private bool VerifyBackup()
    {
        if (HasValidationErrors(out var errors))
        {
            HighlightErrors?.Invoke(errors);
            return false;
        }
        return true;
    }
    private void Create()
    {
        if (!VerifyBackup())
            return;
        SaveTask newBackup = new SaveTask(_backupSource, _backupDestination, _backupName, _backupType);
        _backupManager.AddBackup(newBackup);
        CloseWindow();
    }
    private void Save()
    {
        if (_oldBackupName != _backupName && !_backupManager.BackupExists(_backupName))
        {
            _backupManager.RenameBackup(_oldBackupName, _backupName);
        }
        SaveTask updatedBackup = new SaveTask(_backupSource, _backupDestination, _backupName, _backupType);
        _backupManager.EditBackup(updatedBackup);
        CloseWindow();
    }
    private void Cancel() => CloseWindow();

    private void CloseWindow() => RequestClose?.Invoke();

    public bool HasValidationErrors(out List<string> errors)
    {
        errors = new();

        if (string.IsNullOrWhiteSpace(BackupName))
            errors.Add(nameof(BackupName));
        else if (_backupManager.BackupExists(BackupName))
            errors.Add(nameof(BackupName));

        if (string.IsNullOrWhiteSpace(BackupSource))
            errors.Add(nameof(BackupSource));
        else if (!Directory.Exists(BackupSource))
            errors.Add("InvalidBackupSource");
        if (string.IsNullOrWhiteSpace(BackupDestination))
            errors.Add(nameof(BackupDestination));

        return errors.Count > 0;
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}