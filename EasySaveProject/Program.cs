using EasySaveProject.ViewModels;
using EasySaveProject.Views;

class Program
{
    static void Main()
    {
        var viewModel = new MenuViewModel();
        var view = new MainMenuView(viewModel);
        view.Show();
    }
}
