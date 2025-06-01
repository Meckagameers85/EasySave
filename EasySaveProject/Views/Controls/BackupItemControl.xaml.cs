using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using EasySaveProject.Models;
using EasySaveProject.ViewModels;

namespace EasySaveProject.Controls
{
    public partial class BackupItemControl : UserControl
    {
        public BackupItemControl(SaveTask task)
        {
            InitializeComponent();

            var viewModel = new BackupItemViewModel(task);
            viewModel.RequestClose += () => Window.GetWindow(this)?.Close();
            DataContext = viewModel;
        }

        // Nettoyage lors de la destruction du contrôle
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BackupItemViewModel viewModel)
            {
                viewModel.Dispose();
            }
        }
    }

    // Convertisseur pour inverser un booléen
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : false;
        }
    }
}