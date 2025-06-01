using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using EasySaveProject;

using EasySaveProject.ViewModels;

namespace EasySaveProject
{
    public partial class BackupWindow : Window
    {
        public BackupWindow()
        {
            InitializeComponent();
            var _dataContext = new BackupWindowViewModel();
            _dataContext.RequestClose += () => this.Close();
            _dataContext.HighlightErrors += HighlightInvalidFields;
            DataContext = _dataContext;
        }

        public BackupWindow(string backupName, string backupSourcePath, string backupTargetPath, string backupType, bool isEncrypted)
        {
            InitializeComponent();

            var _dataContext = new BackupWindowViewModel(backupName, backupSourcePath, backupTargetPath, backupType, isEncrypted);
            _dataContext.RequestClose += () => this.Close();
            _dataContext.HighlightErrors += HighlightInvalidFields;
            DataContext = _dataContext;
        }

        private void HighlightInvalidFields(List<string> fieldNames)
        {
            // Reset all styles (tu peux améliorer ce code si tu veux conserver un état)
            BackupNameBox.ClearValue(Border.BorderBrushProperty);
            BackupSourceBox.ClearValue(Border.BorderBrushProperty);
            BackupDestinationBox.ClearValue(Border.BorderBrushProperty);

            if (fieldNames.Contains(nameof(BackupWindowViewModel.BackupName)))
                BackupNameBox.BorderBrush = System.Windows.Media.Brushes.Red;
            if (fieldNames.Contains(nameof(BackupWindowViewModel.BackupSource)))
                BackupSourceBox.BorderBrush = System.Windows.Media.Brushes.Red;
            if (fieldNames.Contains(nameof(BackupWindowViewModel.BackupDestination)))
                BackupDestinationBox.BorderBrush = System.Windows.Media.Brushes.Red;
            if (fieldNames.Contains("InvalidBackupSource"))
                BackupSourceBox.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.Reload();
            }
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TextBox textBox)
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Choisissez un dossier"
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    textBox.Text = dialog.FileName;
                }
            }
        }
    }
}