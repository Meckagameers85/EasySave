using System.Windows;
using System.Windows.Controls;
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
    }
}
