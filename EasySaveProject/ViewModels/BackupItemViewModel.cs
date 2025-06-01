using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class BackupItemViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly BackupManager _backupManager;
    private readonly LanguageManager _languageManager;
    private readonly DispatcherTimer _stateUpdateTimer;
    private readonly DispatcherTimer _completionResetTimer;
    private bool _disposed = false;

    public ICommand EditBackupCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand StartBackupCommand { get; }
    public ICommand PauseBackupCommand { get; }  
    public ICommand ResumeBackupCommand { get; }   
    public ICommand StopBackupCommand { get; }

    public event Action? RequestClose;
    public event PropertyChangedEventHandler? PropertyChanged;

    private SaveTask _saveTask = null!;
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

    // Propriétés pour la progression
    private double _progressPercentage = 0.0;
    public double ProgressPercentage
    {
        get => _progressPercentage;
        set
        {
            if (Math.Abs(_progressPercentage - value) > 0.01)
            {
                _progressPercentage = value;
                OnPropertyChanged();
            }
        }
    }

    private string _statusMessage = "Prêt";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isRunning = false;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
                InvalidateCommands();
            }
        }
    }

    private bool _isPaused = false;
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (_isPaused != value)
            {
                _isPaused = value;
                OnPropertyChanged();
                InvalidateCommands();
            }
        }
    }

    private bool _isCompleted = false;
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted != value)
            {
                _isCompleted = value;
                OnPropertyChanged();
                
                // Auto-reset après 3 secondes si terminé avec succès
                if (value && StatusMessage.Contains("succès"))
                {
                    _completionResetTimer.Start();
                }
            }
        }
    }

    public BackupItemViewModel(SaveTask save)
    {
        _backupManager = BackupManager.instance;
        _languageManager = LanguageManager.instance;
        SaveTask = save;
        IsSelected = false;

        // Commandes avec conditions robustes
        EditBackupCommand = new RelayCommand(EditBackup, CanEdit);
        DeleteBackupCommand = new RelayCommand(DeleteBackup, CanDelete);
        StartBackupCommand = new RelayCommand(StartBackup, CanStart);
        PauseBackupCommand = new RelayCommand(PauseBackup, CanPause);
        ResumeBackupCommand = new RelayCommand(ResumeBackup, CanResume);
        StopBackupCommand = new RelayCommand(StopBackup, CanStop);

        // Timer principal pour lire state.json
        _stateUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _stateUpdateTimer.Tick += UpdateStateFromFile;
        _stateUpdateTimer.Start();

        // Timer pour reset automatique après completion
        _completionResetTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _completionResetTimer.Tick += ResetAfterCompletion;
    }

    // Conditions d'activation des commandes
    private bool CanEdit() => !IsRunning && !_disposed;
    private bool CanDelete() => !IsRunning && !_disposed;
    private bool CanStart() => !IsRunning && !_disposed;
    private bool CanPause() => IsRunning && !IsPaused && !_disposed;
    private bool CanResume() => IsPaused && !_disposed;
    private bool CanStop() => IsRunning && !_disposed;

    private void InvalidateCommands()
    {
        if (!_disposed)
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                CommandManager.InvalidateRequerySuggested();
            });
        }
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
        get => _saveTask.name ?? "";
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
        get => _saveTask.sourceDirectory ?? "";
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
        get => _saveTask.targetDirectory ?? "";
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

    // Méthodes d'action avec vérifications de sécurité
    private void EditBackup()
    {
        if (!CanEdit()) return;

        try
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
        catch (Exception ex)
        {
            StatusMessage = $"Erreur édition: {ex.Message}";
        }
    }

    private void DeleteBackup()
    {
        if (!CanDelete()) return;

        try
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
        catch (Exception ex)
        {
            StatusMessage = $"Erreur suppression: {ex.Message}";
        }
    }

    public void StartBackup()
    {
        if (!CanStart()) return;

        try
        {
            // Reset immédiat de l'interface
            ProgressPercentage = 0;
            StatusMessage = "Démarrage...";
            IsCompleted = false;
            _completionResetTimer.Stop();

            _backupManager.RunBackup(_saveTask);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur démarrage: {ex.Message}";
        }
    }

    private void PauseBackup()
    {
        if (!CanPause()) return;

        try
        {
            _saveTask.Pause();
            StatusMessage = "Mise en pause...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur pause: {ex.Message}";
        }
    }

    private void ResumeBackup()
    {
        if (!CanResume()) return;

        try
        {
            _saveTask.Resume();
            StatusMessage = "Reprise...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur reprise: {ex.Message}";
        }
    }

    private void StopBackup()
    {
        if (!CanStop()) return;

        try
        {
            _saveTask.Stop();
            StatusMessage = "Arrêt en cours...";
            
            // Mise à jour immédiate pour feedback utilisateur
            IsRunning = false;
            IsPaused = false;
            ProgressPercentage = 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur arrêt: {ex.Message}";
        }
    }

    private void UpdateStateFromFile(object? sender, EventArgs e)
    {
        if (_disposed) return;

        try
        {
            var stateFilePath = SaveTask.s_stateFilePath ?? "state.json";
            
            if (!File.Exists(stateFilePath))
            {
                SetStateToIdle();
                return;
            }

            var json = File.ReadAllText(stateFilePath);
            var states = JsonSerializer.Deserialize<List<SaveState>>(json);
            
            var currentState = states?.FirstOrDefault(s => s.name == _saveTask.name);
            if (currentState != null)
            {
                UpdateFromState(currentState);
            }
            else
            {
                SetStateToIdle();
            }
        }
        catch
        {
            // En cas d'erreur, remettre à l'état idle
            SetStateToIdle();
        }
    }

    private void UpdateFromState(SaveState state)
    {
        // Mettre à jour les états
        IsRunning = state.state == "ACTIVE" || state.state == "PAUSED";
        // ET ajouter cette condition pour détecter le blocage :
        if (state.state == "BLOCKED")
        {
            IsRunning = false;
            IsPaused = false;
        }

        IsPaused = state.state == "PAUSED";
        IsCompleted = state.state == "END";
        
        // Mettre à jour la progression
        ProgressPercentage = state.progression;
        
        // Messages de statut détaillés
        StatusMessage = state.state switch
        {
            "ACTIVE" => CreateActiveMessage(state),
            "PAUSED" => CreatePausedMessage(state),
            "END" => " Terminé avec succès",
            "BLOCKED" => " Bloqué par logiciel métier",
            "STOPPED" => " Arrêtée par l'utilisateur",
            "ERROR" => " Erreur lors de la sauvegarde",
            _ => " Prêt"
        };
    }

    private void SetStateToIdle()
    {
        IsRunning = false;
        IsPaused = false;
        if (!IsCompleted)
        {
            ProgressPercentage = 0;
            StatusMessage = " Prêt";
        }
    }

    private string CreateActiveMessage(SaveState state)
    {
        if (string.IsNullOrEmpty(state.sourceFilePath))
            return " Préparation...";

        var fileName = Path.GetFileName(state.sourceFilePath);
        var filesProcessed = state.totalFilesToCopy - state.nbFilesLeftToDo;
        return $" {fileName} • {filesProcessed}/{state.totalFilesToCopy} fichiers";
    }

    private string CreatePausedMessage(SaveState state)
    {
        if (string.IsNullOrEmpty(state.sourceFilePath))
            return " En pause";

        var fileName = Path.GetFileName(state.sourceFilePath);
        var filesProcessed = state.totalFilesToCopy - state.nbFilesLeftToDo;
        return $" Pause: {fileName} • {filesProcessed}/{state.totalFilesToCopy} fichiers";
    }

    private void ResetAfterCompletion(object? sender, EventArgs e)
    {
        _completionResetTimer.Stop();
        
        if (IsCompleted)
        {
            ProgressPercentage = 0;
            StatusMessage = " Prêt";
            IsCompleted = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _stateUpdateTimer?.Stop();
        _completionResetTimer?.Stop();
        
        // Désabonnement explicite pour éviter les fuites mémoire
        _stateUpdateTimer.Tick -= UpdateStateFromFile;
        _completionResetTimer.Tick -= ResetAfterCompletion;
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (!_disposed)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}