using System.IO;
using Spectre.Console;
using System.Text.Json;

namespace EasySaveConsole.Models;

public class BackupManager
{
    public List<SaveTask> saveTasks { get; set; }
    public string backupFile = "saves.json";

    public BackupManager()
    {
        saveTasks = new List<SaveTask>();
        LoadBackup();
    }

    public void SaveBackup()
    {
        var json = JsonSerializer.Serialize(saveTasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(backupFile, json);
    }
    public void AddBackup(SaveTask saveTask)
    {
        saveTasks.Add(saveTask);
        SaveBackup();
    }

    public List<SaveTask> GetBackups()
    {
        return saveTasks;
    }
    
    public void ClearBackups()
    {
        saveTasks.Clear();
        SaveBackup();
    }

    public void RunBackup(SaveTask saveTask)
    {
        if (string.IsNullOrEmpty(saveTask.sourceDirectory) || string.IsNullOrEmpty(saveTask.targetDirectory))
        {
            AnsiConsole.MarkupLine("[red]Erreur : Répertoire source ou cible manquant.[/]");
            return;
        }

        // Simulate backup process
        AnsiConsole.MarkupLine($"[green]Sauvegarde de \"{saveTask.sourceDirectory}\" vers \"{saveTask.targetDirectory}\" en cours...[/]");
        saveTask.Run();
        AnsiConsole.MarkupLine("[green]Sauvegarde terminée ![/]");
    }
    
    public void LoadBackup()
    {
        if (File.Exists(backupFile))
        {
            try
            {
                var json = File.ReadAllText(backupFile);
                saveTasks = JsonSerializer.Deserialize<List<SaveTask>>(json) ?? new List<SaveTask>();
            }
            catch
            {
                saveTasks = new List<SaveTask>();
                SaveBackup();
            }
        }
        else
        {
            saveTasks = new List<SaveTask>();
            SaveBackup();
        }
    }

    public void EditBackup(SaveTask saveTask)
    {
        var index = saveTasks.FindIndex(s => s.name == saveTask.name);
        if (index != -1)
        {
            saveTasks[index] = saveTask;
            SaveBackup();
        }
    }

    public void DeleteBackup(SaveTask saveTask)
    {
        var index = saveTasks.FindIndex(s => s.name == saveTask.name);
        if (index != -1)
        {
            saveTasks.RemoveAt(index);
            SaveBackup();
        }
    }

    public List<int> ParseIndexes(string input, int max)
    {
        var indexes = new List<int>();

        if (input.Contains('-'))
        {
            var parts = input.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int start) &&
                int.TryParse(parts[1], out int end))
            {
                for (int i = start - 1; i < end && i < max; i++)
                {
                    if (i >= 0) indexes.Add(i);
                }
            }
        }
        else if (input.Contains(';'))
        {
            var parts = input.Split(';');
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int idx) && idx >= 1 && idx <= max)
                {
                    indexes.Add(idx - 1);
                }
            }
        }
        else if (int.TryParse(input, out int single))
        {
            if (single >= 1 && single <= max)
                indexes.Add(single - 1);
        }

        return indexes;
    }

}
