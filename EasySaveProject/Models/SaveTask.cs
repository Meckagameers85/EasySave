using System.Text.Json;
using EasySaveProject.Models;
using System.IO;

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

    public static SettingsManager? s_settingsManager { get; set; }
    private ProcessMonitor? _processMonitor;

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

    public void SetSaveType(string setTypeVar)
    {
        /*
            Visibility : public
            Input : string setTypeVar
            Output : None
            Description : Set the type of save based on the provided string.
        */
        if (_stringToSaveType.TryGetValue(setTypeVar, out var saveType)) { type = saveType; }
        else { type = SaveType.Full; }
    }

    public string GetSaveType()
    {
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

        // 🔄 CORRECTION : Initialisation plus robuste du ProcessMonitor
        InitializeProcessMonitor();
    }

    // 🆕 NOUVELLE méthode pour initialiser le ProcessMonitor
    private void InitializeProcessMonitor()
    {
        if (s_settingsManager != null)
        {
            _processMonitor = new ProcessMonitor(s_settingsManager.businessSoftwareName);
            Console.WriteLine($"[DEBUG] ProcessMonitor initialisé avec: '{s_settingsManager.businessSoftwareName}'");
        }
        else
        {
            Console.WriteLine("[DEBUG] s_settingsManager est null - ProcessMonitor non initialisé");
        }
    }

    public override string ToString()
    {
        /*
            Visibility : public
            Input : None
            Output : string
            Description : Get a string representation of the SaveTask object.
        */
        return $"[bold]{name}[/] ({GetSaveType()}): \"{sourceDirectory}\" ==> \"{targetDirectory}\"";
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

        // 🆕 AJOUT : Réinitialiser le ProcessMonitor au cas où les paramètres auraient changé
        InitializeProcessMonitor();

        // 🆕 AJOUT : VÉRIFICATION AVANT DE COMMENCER (Scénario B)
        if (_processMonitor?.IsBusinessSoftwareRunning() == true)
        {
            var blockEntry = new LoggerLib.LogEntry
            {
                timestamp = DateTime.UtcNow,
                saveName = name ?? "Unnamed",
                source = "BACKUP_BLOCKED",
                destination = $"Backup refused - Business software running: {s_settingsManager?.businessSoftwareName}",
                sizeBytes = 0,
                transferTimeMs = -1
            };
            s_logger?.Log(blockEntry);

            Console.WriteLine($"🚫 SAUVEGARDE BLOQUÉE - Logiciel métier '{s_settingsManager?.businessSoftwareName}' détecté");
            // 🚫 REFUSE DE DÉMARRER - sortie immédiate
            return;
        }

        if (!Directory.Exists(sourceDirectory))
            return;

        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory!);

        var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        long totalSize = files.Sum(f => new FileInfo(f).Length);
        int filesRemaining = totalFiles;

        Console.WriteLine($"✅ Démarrage sauvegarde - {totalFiles} fichiers à traiter");

        foreach (var file in files)
        {

            // je veux faire un sleep de 4s
            // System.Threading.Thread.Sleep(1000);
            // 🆕 VÉRIFICATION PENDANT LA SAUVEGARDE (Scénario C)
            if (_processMonitor?.IsBusinessSoftwareRunning() == true)
            {
                // Log de l'arrêt dans le fichier de log
                var stopEntry = new LoggerLib.LogEntry
                {
                    timestamp = DateTime.UtcNow,
                    saveName = name ?? "Unnamed",
                    source = file,
                    destination = $"STOPPED - Business software detected: {s_settingsManager?.businessSoftwareName}",
                    sizeBytes = 0,
                    transferTimeMs = -1 // Code d'arrêt
                };
                s_logger?.Log(stopEntry);

                Console.WriteLine($"⏹️ SAUVEGARDE INTERROMPUE - Logiciel métier '{s_settingsManager?.businessSoftwareName}' détecté");
                // Arrêt immédiat de la sauvegarde
                break;
            }

            var relativePath = Path.GetRelativePath(sourceDirectory!, file);
            var destinationPath = Path.Combine(targetDirectory!, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            var logEntry = new LoggerLib.LogEntry()
            {
                saveName = name ?? "Unnamed",
                timestamp = DateTime.UtcNow
            };

            if (!Directory.Exists(destinationDir))
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    Directory.CreateDirectory(destinationDir!);
                    var endTime = DateTime.UtcNow;
                    logEntry.transferTimeMs = (endTime - startTime).TotalMilliseconds;
                }
                catch
                {
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
                try
                {
                    File.Copy(file, destinationPath, true);
                    var endTime = DateTime.UtcNow;
                    logEntry.transferTimeMs = (endTime - startTime).TotalMilliseconds;
                }
                catch
                {
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
        Console.WriteLine("✅ Sauvegarde terminée");
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

        SaveState._mutex.WaitOne();
        if (File.Exists(s_stateFilePath))
        {
            try
            {
                var json = File.ReadAllText(s_stateFilePath);
                states = JsonSerializer.Deserialize<List<SaveState>>(json) ?? new List<SaveState>();
            }
            catch
            {
                states = new List<SaveState>();
            }
        }

        var index = states.FindIndex(s => s.name == currentState.name);
        if (index >= 0)
            states[index] = currentState;
        else
            states.Add(currentState);

        try
        {
            var updatedJson = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(s_stateFilePath, updatedJson);
        }
        catch
        {
            // Erreur silencieuse pour ne pas interrompre la sauvegarde
        }
        SaveState._mutex.ReleaseMutex();
    }
}
