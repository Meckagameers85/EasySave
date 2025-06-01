using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasySaveProject.ViewModels;
using EasySaveProject.Controls;

namespace EasySaveProject
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GUIMainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();

            viewModel = new GUIMainViewModel();
            DataContext = viewModel;
            LoadBackups();
        }
        public void Reload()
        {
            viewModel.ReloadTexts();
            viewModel.ReloadBackups();
            viewModel.ReloadSeletcedBackups();
            // viewModel.ReloadSettings();
            // viewModel.ReloadCryptoSoftSettings();
            LoadBackups();
        }
        public void updateSelectedBackups()
        {
            viewModel.ReloadSeletcedBackups();
        }
        private void LoadBackups()
        {
            BackupList.Children.Clear();

            foreach (var item in viewModel.BackupItems)
            {
                BackupList.Children.Add(item);
            }

            MegaNewButton.Visibility = viewModel.BackupItems.Count == 0 ? Visibility.Visible : Visibility.Hidden;
            NewButton.Visibility = viewModel.BackupItems.Count == 0 ? Visibility.Hidden : Visibility.Visible;
        }
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var backupWindow = new BackupWindow();
            backupWindow.Owner = this;
            backupWindow.ShowDialog();
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        private void SettingsCrytoSoftButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsCryptoSoftWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            string text = string.Empty;
            foreach (var item in viewModel.SelectedBackupItems)
            {
                if (item.DataContext is BackupItemViewModel viewModel)
                {
                    text += $"Name: {viewModel.BackupName},";
                }
            }
            System.Windows.MessageBox.Show(text, "Selected Backups", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}