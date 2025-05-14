using Spectre.Console;
using EasySaveConsole.ViewModels;

namespace EasySaveConsole.Views;

public class MainMenuView
{
    private readonly MenuViewModel _viewModel;

    public MainMenuView(MenuViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void Show()
    {
        ShowHeader();

        // Boucle principale du menu
        while (true)
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
        // ASCII logo
        var figlet = new FigletText("EasySave v1.0")
            .Color(Color.Green)
            .Centered();

        AnsiConsole.Write(figlet);

        // Crédit centré
        AnsiConsole.Write(
            new Align(
                new Padder(
                    new Markup("[white]by Etienne & Jade & Mahery & Thomas²[/]"),
                    new Padding(0, 0, 2, 1)),
                HorizontalAlignment.Center));

        AnsiConsole.WriteLine();
    }
}
