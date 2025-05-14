using System.IO;
using Spectre.Console;
using System.Text.Json;

namespace EasySaveProject.Models;

public class BackupManager
{
    public List<SaveTask> saveTasks { get; set; }
    public string backupFile = "saves.json";

    public BackupManager()
    {
        /*
            - Visibility : public
            - Input : None
            - Output : None
            - Description : Constructor of the BackupManager class. It initializes the saveTasks list and loads the backup from the JSON file.
        */
        saveTasks = new List<SaveTask>();
        LoadBackup();
    }

    public void SaveBackup()
    {
        /*
            - Visibility : public
            - Input : None
            - Output : None
            - Description : Save the current state of saveTasks into a JSON file.
        */
        var json = JsonSerializer.Serialize(saveTasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(backupFile, json);
    }
    public void AddBackup(SaveTask saveTask)
    {
        /*
            - Visibility : public
            - Input : SaveTask saveTask
            - Output : None
            - Description : Add a new SaveTask to the saveTasks list and save the backup.
        */
        saveTasks.Add(saveTask);
        SaveBackup();
    }

    public List<SaveTask> GetBackups()
    {
        /*
            - Visibility : public
            - Input : None
            - Output : List<SaveTask>
            - Description : Get the list of SaveTasks.
        */
        return saveTasks;
    }
    
    public void ClearBackups()
    {
        /*
            - Visibility : public
            - Input : None
            - Output : None
            - Description : Clear the list of SaveTasks and save the backup.
        */
        saveTasks.Clear();
        SaveBackup();
    }

    public bool RunBackup(SaveTask saveTask)
    {
        /*
            - Visibility : public
            - Input : SaveTask saveTask
            - Output : bool 
            - Description : Run the backup process for the given SaveTask.
        */
        if (string.IsNullOrEmpty(saveTask.sourceDirectory) || string.IsNullOrEmpty(saveTask.targetDirectory))
        {
            return false;
        }

        AnsiConsole.MarkupLine($"[green]{saveTask.WayToString()}[/]");
        saveTask.Run();
        return true;
    }
    
    public void LoadBackup()
    {
        /*
            - Visibility : public
            - Input : None
            - Output : None
            - Description : Load the backup from the JSON file. If the file does not exist or is invalid, create a new one.
        */
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
        /*
            - Visibility : public
            - Input : SaveTask saveTask
            - Output : None
            - Description : Edit an existing SaveTask in the saveTasks list and save the backup.
        */
        var index = saveTasks.FindIndex(s => s.name == saveTask.name);
        if (index != -1)
        {
            saveTasks[index] = saveTask;
            SaveBackup();
        }
    }

    public void DeleteBackup(SaveTask saveTask)
    {
        /*
            - Visibility : public
            - Input : SaveTask saveTask
            - Output : None
            - Description : Delete a SaveTask from the saveTasks list and save the backup.
        */
        var index = saveTasks.FindIndex(s => s.name == saveTask.name);
        if (index != -1)
        {
            saveTasks.RemoveAt(index);
            SaveBackup();
        }
    }

    public List<int> ParseIndexes(string input, int max)
    {
        /*
            - Visibility : public
            - Input : string input, int max
            - Output : List<int>
            - Description : Parse the input string to extract indexes. The input can be a single index, a range (e.g., "1-3"), 
                            or a list of indexes (e.g., "1;2;3").
        */
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
