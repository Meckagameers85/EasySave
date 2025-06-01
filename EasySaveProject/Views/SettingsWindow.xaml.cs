using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using EasySaveProject;

using EasySaveProject.ViewModels;

namespace EasySaveProject
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var _dataContext = new SettingsWindowViewModel();
            _dataContext.RequestClose += () => this.Close();
            _dataContext.BusinessSoftwareTested += ActionTestBusinessSoftware;
            DataContext = _dataContext;
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
        private void ActionTestBusinessSoftware(string businessSoftware)
        {
            businessSoftwareTestButton.ClearValue(Button.BackgroundProperty);
            if (string.IsNullOrWhiteSpace(businessSoftware))
                businessSoftwareTestButton.Background = System.Windows.Media.Brushes.Red;
            else
                businessSoftwareTestButton.Background = System.Windows.Media.Brushes.Green;
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