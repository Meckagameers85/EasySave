using EasySaveConsole.Models;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using LoggerLib;

namespace EasySaveConsole.ViewModels;

public class MenuViewModel
{
    public List<ActionItem> actions { get; private set; }

    private readonly BackupManager _backupManager;
    private readonly SettingsManager _settingsManager;
    private readonly LanguageManager _localization;
    private readonly Logger _logger;

    public MenuViewModel()
    {
        _settingsManager = new SettingsManager();
        _localization = new LanguageManager();
        _localization.Load(_settingsManager.currentLanguage);
        _backupManager = new BackupManager();
        _logger = new Logger("logs");
        SaveTask.s_logger = _logger;

        actions = new List<ActionItem>
        {
            new(_localization.Translate("menu.create")),
            new(_localization.Translate("menu.show")),
            new(_localization.Translate("menu.load")),
            new(_localization.Translate("menu.edit")),
            new(_localization.Translate("menu.delete")),
            new(_localization.Translate("menu.settings")),
            new(_localization.Translate("menu.quit"))
        };
    }

    public string ExecuteAction(string action)
    {
        return action switch
        {
            var a when a == _localization.Translate("menu.create") => CreateBackup(),
            var a when a == _localization.Translate("menu.show") => ShowBackups(),
            var a when a == _localization.Translate("menu.load") => LoadBackup(),
            var a when a == _localization.Translate("menu.edit") => EditBackup(),
            var a when a == _localization.Translate("menu.delete") => DeleteBackup(),
            var a when a == _localization.Translate("menu.settings") => ShowSettingsMenu(),
            var a when a == _localization.Translate("menu.quit") => Quit(),
            _ => _localization.Translate("action.unknown")
        };
    }

    private string ShowSettingsMenu()
    {
        while (true)
        {
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_localization.Translate("settings.title"))
                    .PageSize(5)
                    .AddChoices(new[]
                    {
                        _localization.Translate("settings.language"),
                        // _localization.Translate("settings.defaulttype"),
                        // _localization.Translate("settings.resetpaths"),
                        _localization.Translate("menu.quit")
                    }));

            if (option == _localization.Translate("settings.language"))
            {
                var lang = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(_localization.Translate("settings.choose.language"))
                        .AddChoices(new[]
                            {
                                _localization.Translate("lang.en"),
                                _localization.Translate("lang.fr"),
                                _localization.Translate("lang.es"),
                                _localization.Translate("lang.de"),
                                _localization.Translate("lang.it"),
                                _localization.Translate("lang.ru"),
                                _localization.Translate("lang.pt"),
                                _localization.Translate("menu.quit")
                            }
                        ));
                if (lang == _localization.Translate("menu.quit"))
                {
                    return _localization.Translate("lang.set") + " " + _settingsManager.currentLanguage;
                }
                Dictionary<string, string> languageMapCode = new Dictionary<string, string>
                {
                    { _localization.Translate("lang.en"), "en" },
                    { _localization.Translate("lang.fr"), "fr" },
                    { _localization.Translate("lang.es"), "es" },
                    { _localization.Translate("lang.de"), "de" },
                    { _localization.Translate("lang.it"), "it" },
                    { _localization.Translate("lang.ru"), "ru" },
                    { _localization.Translate("lang.pt"), "pt" }
                };
                string langCode = languageMapCode[lang];
                _settingsManager.ChangeLanguage(langCode);
                _localization.Load(langCode);
                RefreshActions();
                return _localization.Translate("lang.set") + " " + _localization.Translate($"lang.{langCode}");
            }
            // else if (option == _localization.Translate("settings.defaulttype"))
            // {
            //     var type = AnsiConsole.Prompt(
            //         new SelectionPrompt<string>()
            //             .Title(_localization.Translate("settings.choose.defaulttype"))
            //             .AddChoices("Complète", "Différentielle"));

            //     _settingsManager.SetDefaultType(type);
            //     return $"Type par défaut défini : {type}";
            // }
            // else if (option == _localization.Translate("settings.resetpaths"))
            // {
            //     _settingsManager.SetLastPaths("", "");
            //     return "Chemins réinitialisés.";
            // }
            else if (option == _localization.Translate("menu.quit"))
            {
                break;
            }
        }

        return "";
    }

    private void RefreshActions()
    {
        actions = new List<ActionItem>
        {
            new(_localization.Translate("menu.create")),
            new(_localization.Translate("menu.show")),
            new(_localization.Translate("menu.load")),
            new(_localization.Translate("menu.edit")),
            new(_localization.Translate("menu.delete")),
            new(_localization.Translate("menu.settings")),
            new(_localization.Translate("menu.quit"))
        };
    }

    private string CreateBackup()
    {
        var save = new SaveTask();

        save.name = AnsiConsole.Ask<string>(_localization.Translate("create.name"));
        _backupManager.GetBackups().ForEach(s =>
        {
            if (s.name == save.name)
            {
                AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorName")}[/]");
                save.name = AnsiConsole.Ask<string>(_localization.Translate("create.name"));
            }
        });

        save.sourceDirectory = AnsiConsole.Ask<string>(_localization.Translate("create.srcPath"));
        while (!Directory.Exists(save.sourceDirectory))
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorPath")}[/]");
            save.sourceDirectory = AnsiConsole.Ask<string>(_localization.Translate("create.srcPath"));
        }

        save.targetDirectory = AnsiConsole.Ask<string>(_localization.Translate("create.destPath"));
        while (!Directory.Exists(save.targetDirectory))
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorPath")}[/]");
            save.targetDirectory = AnsiConsole.Ask<string>(_localization.Translate("create.destPath"));
        }
        save.SetSaveType(AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_localization.Translate("create.type"))
                .AddChoices(
                    _localization.Translate("create.type.full"),
                    _localization.Translate("create.type.differential")
                )
            )
        );
        try
        {
            _backupManager.AddBackup(save);
            AnsiConsole.MarkupLine($"[green]{_localization.Translate("create.succes")}[/]");
        }
        catch (System.Exception)
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.fail")}[/]");
        }
        return save.ToString();
    }

    private string ShowBackups()
    {
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("show.noBackups")}[/]");
            return "";
        }

        var quitLabel = _localization.Translate("menu.quit");

        var backupChoices = backups.Cast<object>().ToList();
        backupChoices.Add(quitLabel);

        var selectedItems = AnsiConsole.Prompt(
            new MultiSelectionPrompt<object>()
                .Title($"[bold]{_localization.Translate("show.title")}[/]")
                .PageSize(10)
                .InstructionsText($"[grey]{_localization.Translate("show.instruction")}[/]")
                .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                .AddChoices(backupChoices)
        );

        // Si "Quitter" est sélectionné (même avec autre chose), ne rien faire et quitter
        if (selectedItems.Any(item => item is string s && s == quitLabel))
        {
            return "";
        }

        var selectedBackups = selectedItems.OfType<SaveTask>().ToList();
        if (selectedBackups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{_localization.Translate("show.noSelected")}[/]");
            return "";
        }

        ShowBackupTable(selectedBackups, _localization.Translate("show.selected"));
        return "";
    }

    private void ShowBackupTable(List<SaveTask> backups, string title, bool showID = false)
    {
        Dictionary<string, string> typeMap = new Dictionary<string, string>
        {
            { "Full", _localization.Translate("create.type.full") },
            { "Differential", _localization.Translate("create.type.differential")}
        };
        var table = new Table()
            .Title($"[underline green]{title}[/]")
            .Border(TableBorder.Rounded);

        if (showID)
        {
            table.AddColumn($"{_localization.Translate("backup.id")}");
        }

        table.AddColumn($"{_localization.Translate("backup.name")}")
            .AddColumn($"{_localization.Translate("backup.type")}")
            .AddColumn($"{_localization.Translate("backup.source")}")
            .AddColumn($"{_localization.Translate("backup.target")}");

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
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("show.noBackups")}[/]");
            return "";
        }

        ShowBackupTable(backups, _localization.Translate("show.backups"), true);
        var input = AnsiConsole.Ask<string>(_localization.Translate("load.prompt"));
        var indexes = _backupManager.ParseIndexes(input, backups.Count);

        foreach (var i in indexes)
        {
            var backup = backups[i];
            AnsiConsole.MarkupLine($"[green]{_localization.Translate("load.backupRun")}{i + 1} : {Markup.Escape(backup.name ?? "")}[/]");
            _backupManager.RunBackup(backup);
        }

        return $"[green]{_localization.Translate("load.success")}{string.Join(", ", indexes.Select(i => i + 1))}[/]";
    }

    private string EditBackup()
    {
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("show.noBackups")}[/]");
            return "";
        }
        while (true)
        {

            var quitLabel = _localization.Translate("menu.quit");

            var choices = backups.Cast<object>().ToList();
            choices.Add(quitLabel);

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title(_localization.Translate("edit.select"))
                    .PageSize(10)
                    .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                    .AddChoices(choices)
            );

            if (selected is string s && s == quitLabel) { break; }

            else if (selected is not SaveTask backupToEdit)
            {
                AnsiConsole.MarkupLine($"[yellow]{_localization.Translate("show.noSelected")}[/]");
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
                    AnsiConsole.MarkupLine($"[green]{_localization.Translate("edit.selected")}{Markup.Escape(backupToEdit.name ?? "")}[/]");

                    string editTitle = $"{backupToEdit.name} :";
                    List<SaveTask> table = new List<SaveTask> { backupToEdit };
                    ShowBackupTable(table, editTitle);

                    var editChoices = new List<string> {
                        _localization.Translate("backup.name"),
                        _localization.Translate("backup.source"),
                        _localization.Translate("backup.target"),
                        _localization.Translate("backup.type")
                    };
                    editChoices.Add(quitLabel);

                    var editSelected = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"[bold]{_localization.Translate("edit.title")}[/]")
                            .AddChoices(editChoices)
                    );

                    if (editSelected == _localization.Translate("backup.name"))
                    {
                        string newName = AnsiConsole.Ask<string>(_localization.Translate("edit.newName"));
                        while (!IsNameValid(newName))
                        {
                            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorName")}[/]");
                            newName = AnsiConsole.Ask<string>(_localization.Translate("edit.newName"));
                        }
                        backupToEdit.name = newName;
                    }
                    else if (editSelected == _localization.Translate("backup.source"))
                    {
                        backupToEdit.sourceDirectory = AnsiConsole.Ask<string>(_localization.Translate("edit.newSource"));
                        while (!Directory.Exists(backupToEdit.sourceDirectory))
                        {
                            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorPath")}[/]");
                            backupToEdit.sourceDirectory = AnsiConsole.Ask<string>(_localization.Translate("edit.newSource"));
                        }
                    }
                    else if (editSelected == _localization.Translate("backup.target"))
                    {
                        backupToEdit.targetDirectory = AnsiConsole.Ask<string>(_localization.Translate("edit.newTarget"));
                        while (!Directory.Exists(backupToEdit.targetDirectory))
                        {
                            AnsiConsole.MarkupLine($"[red]{_localization.Translate("create.errorPath")}[/]");
                            backupToEdit.targetDirectory = AnsiConsole.Ask<string>(_localization.Translate("create.destPath"));
                        }
                    }
                    else if (editSelected == _localization.Translate("backup.type"))
                    {
                        backupToEdit.SetSaveType(AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title(_localization.Translate("edit.newType"))
                                .AddChoices(
                                    _localization.Translate("create.type.full"),
                                    _localization.Translate("create.type.differential")
                                )
                            )
                        );
                    }
                    else if (editSelected == quitLabel)
                    {
                        _backupManager.EditBackup(backupToEdit);
                        AnsiConsole.MarkupLine($"[green]{_localization.Translate("edit.success")}[/]");
                        break;
                    }
                }
            }

        }
        return "";
    }

    private string DeleteBackup()
    {
        var backups = _backupManager.GetBackups();
        if (backups == null || backups.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Translate("show.noBackups")}[/]");
            return "";
        }
        var allLabel = _localization.Translate("show.all");
        var quitLabel = _localization.Translate("menu.quit");

        var choices = backups.Cast<object>().ToList();
        choices.Add(allLabel);
        choices.Add(quitLabel);

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<object>()
                .Title(_localization.Translate("delete.select"))
                .PageSize(10)
                .UseConverter(item => item is SaveTask task ? task.name ?? "" : item?.ToString() ?? "")
                .AddChoices(choices)
        );

        // Si l'utilisateur choisit "Quitter"
        if (selected is string s && s == quitLabel)
        {
            return "";
        }
        if (selected is string s2 && s2 == allLabel)
        {
            var confirmAll = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_localization.Translate("delete.confirmAll"))
                    .AddChoices(
                        _localization.Translate("delete.yes"),
                        _localization.Translate("delete.no")
                    ));

            if (confirmAll == _localization.Translate("delete.yes"))
            {
                _backupManager.ClearBackups();
                AnsiConsole.MarkupLine($"[green]{_localization.Translate("delete.success")}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]{_localization.Translate("delete.cancel")}[/]");
            }
            return "";
        }

        if (selected is not SaveTask backupToDelete)
        {
            AnsiConsole.MarkupLine($"[yellow]{_localization.Translate("show.noneSelected")}[/]");
            return "";
        }

        // Confirmation
        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{_localization.Translate("delete.confirm")} \"{Markup.Escape(backupToDelete.name ?? "")}\" ?")
                .AddChoices(
                    _localization.Translate("delete.yes"),
                    _localization.Translate("delete.no")
                ));

        if (confirm == _localization.Translate("delete.yes"))
        {
            _backupManager.DeleteBackup(backupToDelete);
            AnsiConsole.MarkupLine($"[green]{_localization.Translate("delete.success")}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]{_localization.Translate("delete.cancel")}[/]");
        }

        return "";
    }

    private string Quit()
    {
        Environment.Exit(0);
        return "A++";
    }

    public string GetTranslated(string key) => _localization.Translate(key);
}
