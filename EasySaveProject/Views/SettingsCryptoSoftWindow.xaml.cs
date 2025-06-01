using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

using EasySaveProject.ViewModels;
namespace EasySaveProject
{
    public partial class SettingsCryptoSoftWindow : Window
    {
        public SettingsCryptoSoftWindow()
        {
            InitializeComponent();
            var _dataContext = new SettingsCryptosoftWindowViewModel();
            _dataContext.RequestClose += () => this.Close();
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