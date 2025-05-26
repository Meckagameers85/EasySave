using System.Text.Json;

public enum SaveType
{
    Full,
    Differential
}

public class SaveTask
{
    public string? name { get; set; }
    public string? sourceDirectory { get; set; }
    public string? targetDirectory { get; set; }

    public SaveType? type { get; set; }

    public static LoggerLib.Logger? s_logger { get; set; }

    public static string s_stateFilePath { get; set; } = "state.json";

    private Dictionary<SaveType, string> _saveTypeToString = new()
    {
        { SaveType.Full, "Full" },
        { SaveType.Differential, "Differential" }
    };

    private Dictionary<string, SaveType> _stringToSaveType = new()
    {
        { "Full", SaveType.Full },
        { "Differential", SaveType.Differential }
    };

    public void SetSaveType(string setTypeVar) {
        /*
            Visibility : public
            Input : string setTypeVar
            Output : None
            Description : Set the type of save based on the provided string.
        */
        if (_stringToSaveType.TryGetValue(setTypeVar, out var saveType)) { type = saveType; } 
        else { type = SaveType.Full; }
    }

    public string GetSaveType() {
        /*
            Visibility : public
            Input : None
            Output : string
            Description : Get the type of save as a string.
        */
        if (_saveTypeToString.TryGetValue(type ?? SaveType.Full, out var typeString)) { return typeString; }
        else { return "Full"; }
    }

    public SaveTask(string? sourceDirectory = null, string? targetDirectory = null, string? name = null, SaveType? type = SaveType.Full)
    {
        /*
            Visibility : public
            Input : string sourceDirectory, string targetDirectory, string name, SaveType type
            Output : None
            Description : Constructor of the SaveTask class. It initializes the source and target directories, name, and type of save.
        */
        this.sourceDirectory = sourceDirectory;
        this.targetDirectory = targetDirectory;
        this.name = name;
        this.type = type;
    }

    public override string ToString()
    {
        /*
            Visibility : public
            Input : None
            Output : string
            Description : Get a string representation of the SaveTask object.
        */
        return $"[bold]{name}[/] ({GetType()}): \"{sourceDirectory}\" ==> \"{targetDirectory}\"";
    }

    public string WayToString()
    {
        /*
            Visibility : public
            Input : None
            Output : string
            Description : Get a string representing the transfer direction of the SaveTask object.
        */
        return $"[bold]\"{sourceDirectory}\"[/] ========> [bold]\"{targetDirectory}\"[/]";
    }

    public void Run()
    {
        /*
            Visibility : public
            Input : None
            Output : None
            Description : Run the backup process for the current SaveTask object.
        */
        if (!Directory.Exists(sourceDirectory))
            return;

        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory!);

        var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        long totalSize = files.Sum(f => new FileInfo(f).Length);
        int filesRemaining = totalFiles;

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourceDirectory!, file);
            var destinationPath = Path.Combine(targetDirectory!, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            var logEntry = new LoggerLib.LogEntry() {
                saveName = name ?? "Unnamed",
                timestamp = DateTime.UtcNow
            };

            if (!Directory.Exists(destinationDir)) {
                var startTime = DateTime.UtcNow;
                try {
                    Directory.CreateDirectory(destinationDir!);
                    var endTime = DateTime.UtcNow;
                    logEntry.transferTimeMs = (endTime - startTime).TotalMilliseconds;
                }
                catch {
                    logEntry.transferTimeMs = -1;
                }
                logEntry.source = Path.Combine(sourceDirectory!, Path.GetDirectoryName(relativePath) ?? "");
                logEntry.destination = destinationDir;
                logEntry.sizeBytes = 0;
                s_logger?.Log(logEntry);

            }
            logEntry.source = file;
            logEntry.destination = destinationPath;
            logEntry.sizeBytes = new FileInfo(file).Length;
            bool shouldCopy = type == SaveType.Full;

            if (type == SaveType.Differential)
            {
                if (!File.Exists(destinationPath) || 
                    File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(destinationPath))
                {
                    shouldCopy = true;
                }
            }

            if (shouldCopy)
            {
                var startTime = DateTime.UtcNow;
                try {
                    File.Copy(file, destinationPath, true);

                    var endTime = DateTime.UtcNow;
                    logEntry.transferTimeMs = (endTime - startTime).TotalMilliseconds;
                }
                catch {
                    logEntry.transferTimeMs = -1;
                }
            }
            s_logger?.Log(logEntry);

            filesRemaining--;
            int progression = (int)(((double)(totalFiles - filesRemaining) / totalFiles) * 100);

            var state = new SaveState
            {
                name = name ?? "Unnamed",
                sourceFilePath = file,
                targetFilePath = destinationPath,
                state = "ACTIVE",
                totalFilesToCopy = totalFiles,
                totalFilesSize = totalSize,
                nbFilesLeftToDo = filesRemaining,
                progression = progression
            };

            UpdateRealtimeState(state);
        }

        var finalState = new SaveState 
        {
            name = name ?? "Unnamed",
            sourceFilePath = "",
            targetFilePath = "",
            state = "END",
            totalFilesToCopy = totalFiles,
            totalFilesSize = totalSize,
            nbFilesLeftToDo = 0,
            progression = 100
        };

        UpdateRealtimeState(finalState);
    }

    private void UpdateRealtimeState(SaveState currentState)
    {
        /*
            Visibility : private
            Input : SaveState currentState
            Output : None
            Description : Update the real-time state of the backup process by saving it into a JSON file.
        */
        List<SaveState> states = new();

        if (File.Exists(s_stateFilePath))
        {
            var json = File.ReadAllText(s_stateFilePath);
            states = JsonSerializer.Deserialize<List<SaveState>>(json) ?? new List<SaveState>();
        }

        var index = states.FindIndex(s => s.name == currentState.name);
        if (index >= 0)
            states[index] = currentState;
        else
            states.Add(currentState);

        var updatedJson = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(s_stateFilePath, updatedJson);
    }
}