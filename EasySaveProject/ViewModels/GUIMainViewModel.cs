using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySaveProject.Models;
using EasySaveProject.ViewModels;
using LoggerLib;
using EasySaveProject.Controls;


namespace EasySaveProject.ViewModels
{

    public class GUIMainViewModel : INotifyPropertyChanged
    {
        private readonly BackupManager _backupManager;
        private readonly SettingsManager _settingsManager;
        private readonly LanguageManager _languageManager;
        private readonly LoggerLib.Logger _logger;
        public ICommand SelectAllBackupCommand { get; }
        public ICommand DeleteSelectedBackupsCommand { get; }
        public ICommand ExecuteSelectedBackupsCommand { get; }
        public ObservableCollection<BackupItemControl> BackupItems { get; set; }
        public ObservableCollection<BackupItemControl> SelectedBackupItems { get; set; }

        public GUIMainViewModel()
        {
            _settingsManager = SettingsManager.instance;
            _languageManager = LanguageManager.instance;
            _languageManager.Load(_settingsManager.currentLanguage);
            _backupManager = BackupManager.instance;
            _logger = new LoggerLib.Logger("logs", _settingsManager.formatLogger);
            SaveTask.s_logger = _logger;
            SaveTask.s_settingsManager = _settingsManager;

            SelectAllBackupCommand = new RelayCommand(SelectAllBackups);
            DeleteSelectedBackupsCommand = new RelayCommand(DeleteSelectedBackups);
            ExecuteSelectedBackupsCommand = new RelayCommand(ExecuteSelectedBackups);
            BackupItems = new ObservableCollection<BackupItemControl>(
                _backupManager.GetBackups().Select(task => new BackupItemControl(task))
            );
            SelectedBackupItems = new ObservableCollection<BackupItemControl>();
        }

        public void ReloadBackups()
        {
            BackupItems.Clear();
            foreach (var task in _backupManager.GetBackups())
            {
                BackupItems.Add(new BackupItemControl(task));
            }
        }

        public void ReloadSeletcedBackups()
        {
            SelectedBackupItems.Clear();
            foreach (var item in BackupItems)
            {
                if (item.DataContext is BackupItemViewModel viewModel && viewModel.IsSelected)
                {
                    SelectedBackupItems.Add(item);
                }
            }
            _isAllSelected = SelectedBackupItems.Count == BackupItems.Count;
        }
        private bool _isAllSelected;
        private void SelectAllBackups()
        {
            if (_isAllSelected)
            {
                foreach (var item in BackupItems)
                {
                    if (item.DataContext is BackupItemViewModel viewModel)
                    {
                        viewModel.IsSelected = false;
                    }
                }
                _isAllSelected = false;
            }
            else
            {
                foreach (var item in BackupItems)
                {
                    if (item.DataContext is BackupItemViewModel viewModel)
                    {
                        viewModel.IsSelected = true;
                    }
                }
                _isAllSelected = true;
            }
            ReloadSeletcedBackups();
        }

        public void DeleteSelectedBackups()
        {
            if (SelectedBackupItems.Count == 0)
                return;
            var names = SelectedBackupItems
                .Select(item => (item.DataContext as BackupItemViewModel)?.BackupName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            string nameList = string.Join(", ", names);

            var confirmation = System.Windows.MessageBox.Show(
                _languageManager.Translate("DeleteSelectedBackupsConfirmation") + " " + nameList,
                _languageManager.Translate("Confirmation"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning
            );

            if (confirmation != System.Windows.MessageBoxResult.Yes)
                return;

            foreach (var item in SelectedBackupItems)
            {
                if (item.DataContext is BackupItemViewModel viewModel)
                {
                    _backupManager.DeleteBackup(viewModel.SaveTask);
                }
            }
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Reload();
            }
        }
        public void ExecuteSelectedBackups()
        {
            if (SelectedBackupItems.Count == 0)
                return;

            foreach (var item in SelectedBackupItems)
            {
                if (item.DataContext is BackupItemViewModel viewModel)
                {
                    viewModel.StartBackup();
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 