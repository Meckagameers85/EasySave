using System.ComponentModel;
using System.Windows.Input;
using System.IO;

using EasySaveProject.Models;

namespace EasySaveProject.ViewModels;

public class BackupItemViewModel : INotifyPropertyChanged
{
    private readonly BackupManager _backupManager;
    private readonly LanguageManager _languageManager;
    public ICommand EditBackupCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand StartBackupCommand { get; }
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

        EditBackupCommand = new RelayCommand(EditBackup);
        DeleteBackupCommand = new RelayCommand(DeleteBackup);
        StartBackupCommand = new RelayCommand(StartBackup);
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
    public void StartBackup()
    {
        _backupManager.RunBackup(_saveTask);
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}