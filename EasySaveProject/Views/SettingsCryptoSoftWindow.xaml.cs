using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EasySaveProject
{
    public partial class SettingsCryptoSoftWindow : Window
    {
        public SettingsCryptoSoftWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.Reload();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Logique de sauvegarde ici
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Logique de réinitialisation ici
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