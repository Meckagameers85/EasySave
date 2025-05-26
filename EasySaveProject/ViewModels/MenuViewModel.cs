using EasySaveProject.Models;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using LoggerLib;

namespace EasySaveProject.ViewModels;

public class MenuViewModel
{
    public List<ActionItem> actions { get; private set; }

    private readonly BackupManager _backupManager;
    private readonly SettingsManager _settingsManager;
    private readonly LanguageManager _languageManager;
    private readonly Logger _logger;

    public MenuViewModel()
    {
        /*
            Visibility : public
            Input : None
            Output : None
            Description : Constructor of the MenuViewModel class. Initializes the settings manager, language manager, backup manager, and logger.
        */
        _settingsManager = new SettingsManager();
        _languageManager = new LanguageManager();
        _languageManager.Load(_settingsManager.currentLanguage);
        _backupManager = new BackupManager();
        _logger = new Logger("logs", _settingsManager.formatLogger);
        SaveTask.s_logger = _logger;

        actions = new List<ActionItem>
        {
            new(_languageManager.Translate("menu.create")),
            new(_languageManager.Translate("menu.show")),
            new(_languageManager.Translate("menu.load")),
            new(_languageManager.Translate("menu.edit")),
            new(_languageManager.Translate("menu.delete")),
            new(_languageManager.Translate("menu.settings")),
            new(_languageManager.Translate("menu.quit"))
        };
    }

    public string ExecuteAction(string action)
    {
        /*
            Visibility : public
            Input : string action
            Output : string
            Description : Executes the action selected by the user in the main menu.
        */
        return action switch
        {
            var a when a == _languageManager.Translate("menu.create") => CreateBackup(),
            var a when a == _languageManager.Translate("menu.show") => ShowBackups(),
            var a when a == _languageManager.Translate("menu.load") => LoadBackup(),
            var a when a == _languageManager.Translate("menu.edit") => EditBackup(),
            var a when a == _languageManager.Translate("menu.delete") => DeleteBackup(),
            var a when a == _languageManager.Translate("menu.settings") => ShowSettingsMenu(),
            var a when a == _languageManager.Translate("menu.quit") => Quit(),
            _ => _languageManager.Translate("action.unknown")
        };
    }

    private string ShowSettingsMenu()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Displays the settings menu and allows the user to change the language.
        */
        while (true)
        {
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_languageManager.Translate("settings.title"))
                    .PageSize(5)
                    .AddChoices(new[]
                    {
                        _languageManager.Translate("settings.language"),
                        _languageManager.Translate("settings.format"),
                        _languageManager.Translate("menu.quit")
                    }));

            if (option == _languageManager.Translate("settings.language"))
            {
                var lang = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(_languageManager.Translate("settings.choose.language"))
                        .AddChoices(new[]
                            {
                                _languageManager.Translate("lang.en"),
                                _languageManager.Translate("lang.fr"),
                                _languageManager.Translate("lang.es"),
                                _languageManager.Translate("lang.de"),
                                _languageManager.Translate("lang.it"),
                                _languageManager.Translate("lang.ru"),
                                _languageManager.Translate("lang.pt"),
                                _languageManager.Translate("menu.quit")
                            }
                        ));
                if (lang == _languageManager.Translate("menu.quit"))
                {
                    return _languageManager.Translate("lang.set") + " " + _settingsManager.currentLanguage;
                }
                Dictionary<string, string> languageMapCode = new Dictionary<string, string>
                {
                    { _languageManager.Translate("lang.en"), "en" },
                    { _languageManager.Translate("lang.fr"), "fr" },
                    { _languageManager.Translate("lang.es"), "es" },
                    { _languageManager.Translate("lang.de"), "de" },
                    { _languageManager.Translate("lang.it"), "it" },
                    { _languageManager.Translate("lang.ru"), "ru" },
                    { _languageManager.Translate("lang.pt"), "pt" }
                };
                string langCode = languageMapCode[lang];
                _settingsManager.ChangeLanguage(langCode);
                _languageManager.Load(langCode);
                RefreshActions();
                return _languageManager.Translate("lang.set") + " " + _languageManager.Translate($"lang.{langCode}");
            }
            else if (option == _languageManager.Translate("settings.format"))
            {
                var format = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(_languageManager.Translate("settings.choose.format"))
                        .AddChoices(new[]
                            {
                                _languageManager.Translate("format.json"),
                                _languageManager.Translate("format.xml"),
                                _languageManager.Translate("menu.quit")
                            }
                        ));
                if (format == _languageManager.Translate("menu.quit"))
                {
                    return _languageManager.Translate("format.set") + " " + _settingsManager.formatLogger;
                }
                Dictionary<string, string> formatMapCode = new Dictionary<string, string>
                {
                    { _languageManager.Translate("format.json"), "JSON" },
                    { _languageManager.Translate("format.xml"), "XML" }
                };
                string formatCode = formatMapCode[format];
                _settingsManager.ChangeFormatLogger(formatCode);
                _logger.logFormat = formatCode;
                RefreshActions();
                return _languageManager.Translate("format.set") + " " + _languageManager.Translate($"format.{formatCode.ToLower()}");
            }
            else if (option == _languageManager.Translate("menu.quit"))
            {
                break;
            }
        }

        return "";
    }

    private void RefreshActions()
    {
        /*
            Visibility : private
            Input : None
            Output : None
            Description : Refresh the actions list with the current language.
        */
        actions = new List<ActionItem>
        {
            new(_languageManager.Translate("menu.create")),
            new(_languageManager.Translate("menu.show")),
            new(_languageManager.Translate("menu.load")),
            new(_languageManager.Translate("menu.edit")),
            new(_languageManager.Translate("menu.delete")),
            new(_languageManager.Translate("menu.settings")),
            new(_languageManager.Translate("menu.quit"))
        };

    }

    private string CreateBackup()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Creates a new backup task by asking the user for the name, source directory, target directory, and backup type.
        */
        var save = new SaveTask();

        save.name = AnsiConsole.Ask<string>(_languageManager.Translate("create.name"));
        _backupManager.GetBackups().ForEach(s =>
        {
            if (s.name == save.name)
            {
                AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorName")}[/]");
                save.name = AnsiConsole.Ask<string>(_languageManager.Translate("create.name"));
            }
        });

        save.sourceDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("create.srcPath"));
        while (!Directory.Exists(save.sourceDirectory))
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorPath")}[/]");
            save.sourceDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("create.srcPath"));
        }

        save.targetDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("create.destPath"));
        while (true)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(save.targetDirectory)) {
                isValid = false;
            }
            else {
                try {
                    foreach (char c in Path.GetInvalidPathChars()) {
                        if (save.targetDirectory.Contains(c)) {
                            isValid = false;
                            break;
                        }
                    }
                }
                catch {
                    isValid = false;
                }
            }

            if (isValid) break;

            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorPath")}[/]");
            save.targetDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("create.destPath"));
        }
        save.SetSaveType(AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_languageManager.Translate("create.type"))
                .AddChoices(
                    _languageManager.Translate("create.type.full"),
                    _languageManager.Translate("create.type.differential")
                )
            )
        );
        try
        {
            _backupManager.AddBackup(save);
            AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("create.succes")}[/]");
        }
        catch (System.Exception)
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.fail")}[/]");
        }
        return save.ToString();
    }

    private string ShowBackups()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Displays the list of backups and allows the user to select one or more backups to show their details.
        */
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("show.noBackups")}[/]");
            return "";
        }

        var quitLabel = _languageManager.Translate("menu.quit");

        var backupChoices = backups.Cast<object>().ToList();
        backupChoices.Add(quitLabel);

        var selectedItems = AnsiConsole.Prompt(
            new MultiSelectionPrompt<object>()
                .Title($"[bold]{_languageManager.Translate("show.title")}[/]")
                .PageSize(10)
                .InstructionsText($"[grey]{_languageManager.Translate("show.instruction")}[/]")
                .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                .AddChoices(backupChoices)
        );

        if (selectedItems.Any(item => item is string s && s == quitLabel))
        {
            return "";
        }

        var selectedBackups = selectedItems.OfType<SaveTask>().ToList();
        if (selectedBackups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{_languageManager.Translate("show.noSelected")}[/]");
            return "";
        }

        ShowBackupTable(selectedBackups, _languageManager.Translate("show.selected"));
        return "";
    }

    private void ShowBackupTable(List<SaveTask> backups, string title, bool showID = false)
    {
        /*
            Visibility : private
            Input : List<SaveTask> backups, string title, bool showID
            Output : None
            Description : Displays a table with the details of the backups.
        */
        Dictionary<string, string> typeMap = new Dictionary<string, string>
        {
            { "Full", _languageManager.Translate("create.type.full") },
            { "Differential", _languageManager.Translate("create.type.differential")}
        };
        var table = new Table()
            .Title($"[underline green]{title}[/]")
            .Border(TableBorder.Rounded);

        if (showID)
        {
            table.AddColumn($"{_languageManager.Translate("backup.id")}");
        }

        table.AddColumn($"{_languageManager.Translate("backup.name")}")
            .AddColumn($"{_languageManager.Translate("backup.type")}")
            .AddColumn($"{_languageManager.Translate("backup.source")}")
            .AddColumn($"{_languageManager.Translate("backup.target")}");

        int id = 1;
        foreach (var backup in backups)
        {
            if (showID)
            {
                table.AddRow(
                    Markup.Escape(id.ToString()),
                    Markup.Escape(backup.name ?? ""),
                    Markup.Escape(typeMap[backup.GetSaveType()]),
                    Markup.Escape(backup.sourceDirectory ?? ""),
                    Markup.Escape(backup.targetDirectory ?? "")
                );
            }
            else
            {
                table.AddRow(
                    Markup.Escape(backup.name ?? ""),
                    Markup.Escape(typeMap[backup.GetSaveType()]),
                    Markup.Escape(backup.sourceDirectory ?? ""),
                    Markup.Escape(backup.targetDirectory ?? "")
                );
            }
            id++;
        }

        AnsiConsole.Write(table);
    }

    private string LoadBackup()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Run a backup by allowing the user to select one or more backups to run.
        */
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("show.noBackups")}[/]");
            return "";
        }

        ShowBackupTable(backups, _languageManager.Translate("show.backups"), true);
        var input = AnsiConsole.Ask<string>(_languageManager.Translate("load.prompt"));
        var indexes = _backupManager.ParseIndexes(input, backups.Count);

        foreach (var i in indexes)
        {
            var backup = backups[i];
            AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("load.backupRun")}{i + 1} : {Markup.Escape(backup.name ?? "")}[/]");
            bool success = _backupManager.RunBackup(backup);
            if (success) { AnsiConsole.MarkupLine($"[green]    {_languageManager.Translate("load.success")}[/]"); }
            else { AnsiConsole.MarkupLine($"[red]   {_languageManager.Translate("load.missingPath")}[/]"); }
        }

        return $"[green]{_languageManager.Translate("load.finished")}{string.Join(", ", indexes.Select(i => i + 1))}[/]";
    }

    private string EditBackup()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Edits a backup by allowing the user to change its name, source directory, target directory, and backup type.
        */
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("show.noBackups")}[/]");
            return "";
        }
        while (true)
        {

            var quitLabel = _languageManager.Translate("menu.quit");

            var choices = backups.Cast<object>().ToList();
            choices.Add(quitLabel);

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title(_languageManager.Translate("edit.select"))
                    .PageSize(10)
                    .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                    .AddChoices(choices)
            );

            if (selected is string s && s == quitLabel) { break; }

            else if (selected is not SaveTask backupToEdit)
            {
                AnsiConsole.MarkupLine($"[yellow]{_languageManager.Translate("show.noSelected")}[/]");
            }

            else
            {
                bool IsNameValid(string name)
                {
                    foreach (var backup in backups)
                    {
                        Console.WriteLine($"Name: {backup.name} - Input: {name}");
                        if (backup.name == name)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                while (true)
                {
                    AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("edit.selected")}{Markup.Escape(backupToEdit.name ?? "")}[/]");

                    string editTitle = $"{backupToEdit.name} :";
                    List<SaveTask> table = new List<SaveTask> { backupToEdit };
                    ShowBackupTable(table, editTitle);

                    var editChoices = new List<string> {
                        _languageManager.Translate("backup.name"),
                        _languageManager.Translate("backup.source"),
                        _languageManager.Translate("backup.target"),
                        _languageManager.Translate("backup.type")
                    };
                    editChoices.Add(quitLabel);

                    var editSelected = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"[bold]{_languageManager.Translate("edit.title")}[/]")
                            .AddChoices(editChoices)
                    );

                    if (editSelected == _languageManager.Translate("backup.name"))
                    {
                        string newName = AnsiConsole.Ask<string>(_languageManager.Translate("edit.newName"));
                        while (!IsNameValid(newName))
                        {
                            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorName")}[/]");
                            newName = AnsiConsole.Ask<string>(_languageManager.Translate("edit.newName"));
                        }
                        backupToEdit.name = newName;
                    }
                    else if (editSelected == _languageManager.Translate("backup.source"))
                    {
                        backupToEdit.sourceDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("edit.newSource"));
                        while (!Directory.Exists(backupToEdit.sourceDirectory))
                        {
                            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorPath")}[/]");
                            backupToEdit.sourceDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("edit.newSource"));
                        }
                    }
                    else if (editSelected == _languageManager.Translate("backup.target"))
                    {
                        backupToEdit.targetDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("edit.newTarget"));
                        while (true)
                        {
                            bool isValid = true;

                            if (string.IsNullOrWhiteSpace(backupToEdit.targetDirectory)) {
                                isValid = false;
                            }
                            else {
                                try {
                                    foreach (char c in Path.GetInvalidPathChars()) {
                                        if (backupToEdit.targetDirectory.Contains(c)) {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                catch {
                                    isValid = false;
                                }
                            }

                            if (isValid) break;
                            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("create.errorPath")}[/]");
                            backupToEdit.targetDirectory = AnsiConsole.Ask<string>(_languageManager.Translate("create.destPath"));
                        }
                    }
                    else if (editSelected == _languageManager.Translate("backup.type"))
                    {
                        backupToEdit.SetSaveType(AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title(_languageManager.Translate("edit.newType"))
                                .AddChoices(
                                    _languageManager.Translate("create.type.full"),
                                    _languageManager.Translate("create.type.differential")
                                )
                            )
                        );
                    }
                    else if (editSelected == quitLabel)
                    {
                        _backupManager.EditBackup(backupToEdit);
                        AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("edit.success")}[/]");
                        break;
                    }
                }
            }

        }
        return "";
    }

    private string DeleteBackup()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Deletes a backup by allowing the user to select one or more backups to delete.
        */
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_languageManager.Translate("show.noBackups")}[/]");
            return "";
        }
        var allLabel = _languageManager.Translate("show.all");
        var quitLabel = _languageManager.Translate("menu.quit");

        var choices = backups.Cast<object>().ToList();
        choices.Add(allLabel);
        choices.Add(quitLabel);

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<object>()
                .Title(_languageManager.Translate("delete.select"))
                .PageSize(10)
                .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                .AddChoices(choices)
        );

        if (selected is string s && s == quitLabel)
        {
            return "";
        }
        if (selected is string s2 && s2 == allLabel)
        {
            var confirmAll = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_languageManager.Translate("delete.confirmAll"))
                    .AddChoices(
                        _languageManager.Translate("delete.yes"),
                        _languageManager.Translate("delete.no")
                    ));

            if (confirmAll == _languageManager.Translate("delete.yes"))
            {
                _backupManager.ClearBackups();
                AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("delete.success")}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]{_languageManager.Translate("delete.cancel")}[/]");
            }
            return "";
        }

        if (selected is not SaveTask backupToDelete)
        {
            AnsiConsole.MarkupLine($"[yellow]{_languageManager.Translate("show.noneSelected")}[/]");
            return "";
        }

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{_languageManager.Translate("delete.confirm")} \"{Markup.Escape(backupToDelete.name ?? "")}\" ?")
                .AddChoices(
                    _languageManager.Translate("delete.yes"),
                    _languageManager.Translate("delete.no")
                ));

        if (confirm == _languageManager.Translate("delete.yes"))
        {
            _backupManager.DeleteBackup(backupToDelete);
            AnsiConsole.MarkupLine($"[green]{_languageManager.Translate("delete.success")}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]{_languageManager.Translate("delete.cancel")}[/]");
        }

        return "";
    }

    private string Quit()
    {
        /*
            Visibility : private
            Input : None
            Output : string
            Description : Quits the application.
        */
        Environment.Exit(0);
        return "Coucou Florent, j'espère que la review du code se passe bien. En te souhaitant une bonne continuation de ta journée. Etant donné que je cette fonction permet de quitter l'application... 🤡";
    }

    public string GetTranslated(string key) => _languageManager.Translate(key);
    /*
        Input : string key
        Output : string
        Description : Returns the translated string for the given key.
    */
}
