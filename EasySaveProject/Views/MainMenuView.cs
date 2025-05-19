using Spectre.Console;
using EasySaveProject.ViewModels;

namespace EasySaveProject.Views;

public class MainMenuView
{
    private readonly MenuViewModel _viewModel;

    public MainMenuView(MenuViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void Show()
    {
        /*
            Visibility : public
            Input : None
            Output : None
            Description : Displays the main menu of the EasySave application.
        */
        ShowHeader();

        while (true) // Main menu loop
        {
            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]" + _viewModel.GetTranslated("title.action") + "[/]")
                    .PageSize(10)
                    .AddChoices(_viewModel.actions.Select(a => a.name)));

            var result = _viewModel.ExecuteAction(choix);
            AnsiConsole.MarkupLine($"[green]{result}[/]");
            AnsiConsole.WriteLine();
        }
    }

    private void ShowHeader()
    {
        /*
            Visibility : private
            Input : None
            Output : None
            Description : Displays the header of the EasySave application.
        */

        var figlet = new FigletText("EasySave v1.0.1") // ASCII logo
            .Color(Color.Green)
            .Centered();

        AnsiConsole.Write(figlet);

        AnsiConsole.Write(
            new Align(
                new Padder(
                    new Markup("[white]by Etienne & Jade & Mahery & Thomas²[/]"),
                    new Padding(0, 0, 2, 1)),
                HorizontalAlignment.Center));

        AnsiConsole.WriteLine();
    }
}
