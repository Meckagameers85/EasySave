using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using System.Text.Json;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class BackupItemViewModel : INotifyPropertyChanged
{
    private readonly BackupManager _backupManager;
    private readonly LanguageManager _languageManager;
    private Timer? _stateUpdateTimer; // 🆕 Timer pour lire le state.json

    // 🆕 AJOUT MINIMAL : Progress pour l'UI
    public BackupProgress Progress { get; } = new BackupProgress();

    public ICommand EditBackupCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand StartBackupCommand { get; }
    public ICommand PauseBackupCommand { get; }  // 🆕 Nouveau
    public ICommand StopBackupCommand { get; }   // 🆕 Nouveau

    public event Action? RequestClose;
    public event PropertyChangedEventHandler? PropertyChanged;

    private SaveTask _saveTask;
    public SaveTask SaveTask
    {
        get => _saveTask;
        set
        {
            if (_saveTask != value)
            {
                _saveTask = value;
                OnPropertyChanged();
            }
        }
    }

    public BackupItemViewModel(SaveTask save)
    {
        _backupManager = BackupManager.instance;
        _languageManager = LanguageManager.instance;
        SaveTask = save;
        IsSelected = false;

        EditBackupCommand = new RelayCommand(EditBackup, () => Progress.State != BackupState.Running);
        DeleteBackupCommand = new RelayCommand(DeleteBackup, () => Progress.State != BackupState.Running);
        StartBackupCommand = new RelayCommand(StartBackup, () => Progress.CanPlay);
        PauseBackupCommand = new RelayCommand(PauseBackup, () => Progress.CanPause);
        StopBackupCommand = new RelayCommand(StopBackup, () => Progress.CanStop);

        // 🆕 S'abonner aux changements pour mettre à jour les boutons
        Progress.PropertyChanged += (s, e) => CommandManager.InvalidateRequerySuggested();

        // 🆕 CORRECTION : Lire l'état initial depuis state.json
        LoadInitialStateFromFile();

        // 🆕 CORRECTION : Démarrer un timer pour lire régulièrement le state.json
        _stateUpdateTimer = new Timer(UpdateStateFromFile, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    // 🆕 Méthode pour charger l'état initial
    private void LoadInitialStateFromFile()
    {
        var currentState = ReadCurrentStateFromFile();
        if (currentState != null)
        {
            UpdateProgressFromSaveState(currentState);
        }
    }

    // 🆕 Méthode pour lire périodiquement le state.json
    private void UpdateStateFromFile(object? state)
    {
        try
        {
            var currentState = ReadCurrentStateFromFile();
            if (currentState != null)
            {
                // Mettre à jour sur le thread UI
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    UpdateProgressFromSaveState(currentState);
                });
            }
        }
        catch
        {
            // Ignorer les erreurs de lecture
        }
    }

    // 🆕 Méthode pour lire le state.json
    private SaveState? ReadCurrentStateFromFile()
    {
        var stateFilePath = SaveTask.s_stateFilePath ?? "state.json";

        if (!File.Exists(stateFilePath))
            return null;

        try
        {
            var json = File.ReadAllText(stateFilePath);
            var states = JsonSerializer.Deserialize<List<SaveState>>(json);

            return states?.FirstOrDefault(s => s.name == _saveTask.name);
        }
        catch
        {
            return null;
        }
    }

    // 🆕 Méthode pour mettre à jour le Progress depuis SaveState
    private void UpdateProgressFromSaveState(SaveState saveState)
    {
        // Mettre à jour le pourcentage
        Progress.ProgressPercentage = saveState.progression;

        // Mettre à jour l'état
        Progress.State = saveState.state switch
        {
            "ACTIVE" => BackupState.Running,
            "END" => BackupState.Completed,
            "ERROR" => BackupState.Error,
            "STOPPED" => BackupState.Stopped,
            "PAUSED" => BackupState.Paused,
            _ => BackupState.NotStarted
        };

        // Mettre à jour le message de statut
        if (saveState.state == "ACTIVE")
        {
            var fileName = Path.GetFileName(saveState.sourceFilePath);
            Progress.StatusMessage = $"En cours: {fileName} ({saveState.totalFilesToCopy - saveState.nbFilesLeftToDo}/{saveState.totalFilesToCopy})";
        }
        else if (saveState.state == "END")
        {
            Progress.StatusMessage = "Sauvegarde terminée";
        }
        else if (saveState.state == "ERROR")
        {
            Progress.StatusMessage = "Erreur lors de la sauvegarde";
        }
        else
        {
            Progress.StatusMessage = "Prêt";
        }

        // Mettre à jour les propriétés liées
        OnPropertyChanged(nameof(BackupStateText));
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.updateSelectedBackups();
                }
            }
        }
    }

    public string BackupName
    {
        get => _saveTask.name;
        set
        {
            if (_saveTask.name != value)
            {
                _saveTask.name = value;
                OnPropertyChanged();
            }
        }
    }

    public string BackupSource
    {
        get => _saveTask.sourceDirectory;
        set
        {
            if (_saveTask.sourceDirectory != value)
            {
                _saveTask.sourceDirectory = value;
                OnPropertyChanged();
            }
        }
    }

    public string BackupTarget
    {
        get => _saveTask.targetDirectory;
        set
        {
            if (_saveTask.targetDirectory != value)
            {
                _saveTask.targetDirectory = value;
                OnPropertyChanged();
            }
        }
    }

    public string BackupType
    {
        get => _saveTask.GetSaveType();
        set
        {
            _saveTask.SetSaveType(value);
            OnPropertyChanged();
        }
    }

    private string _backupEncryptType = "None";
    public string BackupEncryptType
    {
        get => _backupEncryptType;
        set
        {
            if (_backupEncryptType != value)
            {
                _backupEncryptType = value;
                OnPropertyChanged();
            }
        }
    }

    // 🆕 Propriétés pour l'état (délègue au Progress)
    public string BackupStateText
    {
        get
        {
            return Progress.State switch
            {
                BackupState.NotStarted => "Prêt",
                BackupState.Running => "En cours",
                BackupState.Paused => "En pause",
                BackupState.Stopped => "Arrêté",
                BackupState.Completed => "Terminé",
                BackupState.Error => "Erreur",
                _ => "Inconnu"
            };
        }
    }

    private void EditBackup()
    {
        var backupWindow = new BackupWindow(
            BackupName,
            BackupSource,
            BackupTarget,
            BackupType,
            BackupEncryptType
        );

        backupWindow.Owner = System.Windows.Application.Current.MainWindow;
        backupWindow.ShowDialog();

        RequestClose?.Invoke();
    }

    private void DeleteBackup()
    {
        var confirm = System.Windows.MessageBox.Show(
            _languageManager.Translate("ConfirmDeleteBackup"),
            _languageManager.Translate("DeleteBackup"),
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning
        );
        if (confirm == System.Windows.MessageBoxResult.Yes)
        {
            _backupManager.DeleteBackup(_saveTask);
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Reload();
            }
        }
    }

    // 🔄 MÉTHODE EXISTANTE : Juste mise à jour du Progress
    public void StartBackup()
    {
        if (Progress.State == BackupState.Paused)
        {
            // Si en pause, juste reprendre la sauvegarde existante
            _saveTask.Resume();
            Progress.State = BackupState.Running;
            Progress.StatusMessage = "Reprise...";
        }
        else if (Progress.State == BackupState.NotStarted || Progress.State == BackupState.Stopped)
        {
            // Si pas encore commencé ou arrêtée, lancer une nouvelle sauvegarde
            Progress.State = BackupState.Running;
            Progress.StatusMessage = "Démarrage...";
            _backupManager.RunBackup(_saveTask);
        }
    }

    // 🆕 NOUVELLES MÉTHODES (pour l'instant, juste des placeholders)
    private void PauseBackup()
    {
        _saveTask.Pause();
        Progress.State = BackupState.Paused;
        Progress.StatusMessage = "En pause...";
    }

    private void StopBackup()
    {
        _saveTask.Stop();
        Progress.State = BackupState.Stopped;
        Progress.StatusMessage = "Arrêtée.";
    }

    // 🆕 Nettoyage du timer
    public void Dispose()
    {
        _stateUpdateTimer?.Dispose();
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}