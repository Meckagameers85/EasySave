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
            
            // CORRECTION : Calculer _isAllSelected basé sur les tâches sélectionnables uniquement
            var selectableItems = BackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && !vm.IsRunning)
                .ToList();
            
            var selectedSelectableItems = selectableItems
                .Where(item => (item.DataContext as BackupItemViewModel)?.IsSelected == true)
                .ToList();
            
            _isAllSelected = selectableItems.Count > 0 && selectedSelectableItems.Count == selectableItems.Count;
        }

        private bool _isAllSelected;
        private void SelectAllBackups()
        {
            // Obtenir seulement les tâches non actives
            var selectableItems = BackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && !vm.IsRunning)
                .ToList();

            // Vérifier si toutes les tâches sélectionnables sont déjà sélectionnées
            bool allSelectableSelected = selectableItems.Count > 0 && 
                selectableItems.All(item => (item.DataContext as BackupItemViewModel)?.IsSelected == true);

            if (allSelectableSelected)
            {
                // Désélectionner toutes les tâches sélectionnables
                foreach (var item in selectableItems)
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
                // Sélectionner toutes les tâches sélectionnables
                foreach (var item in selectableItems)
                {
                    if (item.DataContext is BackupItemViewModel viewModel)
                    {
                        viewModel.IsSelected = true;
                    }
                }
                _isAllSelected = selectableItems.Count > 0;
            }
            
            ReloadSeletcedBackups();
        }

        public void DeleteSelectedBackups()
        {
            if (SelectedBackupItems.Count == 0)
                return;

            // CORRECTION : Filtrer seulement les tâches non actives
            var deletableItems = SelectedBackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && !vm.IsRunning)
                .ToList();

            var runningItems = SelectedBackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && vm.IsRunning)
                .ToList();

            // Message d'information si des tâches actives sont ignorées
            if (runningItems.Count > 0)
            {
                var runningNames = runningItems
                    .Select(item => (item.DataContext as BackupItemViewModel)?.BackupName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                System.Windows.MessageBox.Show(
                    $"{runningItems.Count} sauvegarde(s) en cours ignorée(s) : {string.Join(", ", runningNames)}",
                    "Information",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }

            if (deletableItems.Count == 0)
                return;

            var names = deletableItems
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

            foreach (var item in deletableItems)
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

            // CORRECTION : Filtrer seulement les tâches non actives
            var executableItems = SelectedBackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && !vm.IsRunning)
                .ToList();

            var runningItems = SelectedBackupItems
                .Where(item => item.DataContext is BackupItemViewModel vm && vm.IsRunning)
                .ToList();

            // Message d'information si des tâches actives sont ignorées
            if (runningItems.Count > 0)
            {
                var runningNames = runningItems
                    .Select(item => (item.DataContext as BackupItemViewModel)?.BackupName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                System.Windows.MessageBox.Show(
                    $"{runningItems.Count} sauvegarde(s) déjà en cours ignorée(s) : {string.Join(", ", runningNames)}",
                    "Information",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }

            if (executableItems.Count == 0)
                return;

            foreach (var item in executableItems)
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