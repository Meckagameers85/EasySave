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
    public bool IsEncrypted { get; set; }
    public static SettingsManager? s_settingsManager { get; set; }
    public static CryptoSoftManager? s_cryptoSoftManager { get; set; }
    private ProcessMonitor? _processMonitor;

    // Mécanismes de contrôle play/pause
    private ManualResetEvent _pauseEvent = new(true);
    private CancellationTokenSource _cts = new();

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
        if (_stringToSaveType.TryGetValue(setTypeVar, out var saveType)) { type = saveType; }
        else { type = SaveType.Full; }
    }

    public string GetSaveType()
    {
        if (_saveTypeToString.TryGetValue(type ?? SaveType.Full, out var typeString)) { return typeString; }
        else { return "Full"; }
    }

    public SaveTask(string? sourceDirectory = null, string? targetDirectory = null, string? name = null, SaveType? type = SaveType.Full, bool IsEncrypted = false)
    {
        this.sourceDirectory = sourceDirectory;
        this.targetDirectory = targetDirectory;
        this.name = name;
        this.type = type;
        this.IsEncrypted = IsEncrypted;
        InitializeProcessMonitor();
    }

    private void InitializeProcessMonitor()
    {
        if (s_settingsManager != null)
        {
            _processMonitor = new ProcessMonitor(s_settingsManager.businessSoftwareName);
        }
    }

    public override string ToString()
    {
        return $"[bold]{name}[/] ({GetSaveType()}): \"{sourceDirectory}\" ==> \"{targetDirectory}\"";
    }

    public string WayToString()
    {
        return $"[bold]\"{sourceDirectory}\"[/] ========> [bold]\"{targetDirectory}\"[/]";
    }

    public void Run()
    {
        _cts = new CancellationTokenSource();
        _pauseEvent.Set();
        InitializeProcessMonitor();

        // Vérification avant de commencer
        if (_processMonitor?.IsBusinessSoftwareRunning() == true)
        {
            var blockEntry = new LoggerLib.LogEntry
            {
                timestamp = DateTime.UtcNow,
                saveName = name ?? "Unnamed",
                source = "BACKUP_BLOCKED",
                destination = $"Backup refused - Business software running: {s_settingsManager?.businessSoftwareName}",
                sizeBytes = 0,
                transferTimeMs = -1,
                encryptTimeMs = -1
            };
            s_logger?.Log(blockEntry);
            UpdateStateInFile("BLOCKED");
            return;
        }

        if (!Directory.Exists(sourceDirectory)) return;
        if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory!);

        var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        long totalSize = files.Sum(f => new FileInfo(f).Length);
        int filesRemaining = totalFiles;

        // Liste des gros fichiers reportés
        var deferredLargeFiles = new List<string>();

        // Première passe : traiter tous les fichiers

        foreach (var file in files)
        {
            // Vérifications d'annulation et pause
            if (_cts.Token.IsCancellationRequested) return;
            _pauseEvent.WaitOne();
            if (_cts.Token.IsCancellationRequested) return;

            // Vérification logiciel métier
            InitializeProcessMonitor();
            if (_processMonitor?.IsBusinessSoftwareRunning() == true)
            {
                var stopEntry = new LoggerLib.LogEntry
                {
                    timestamp = DateTime.UtcNow,
                    saveName = name ?? "Unnamed",
                    source = file,
                    destination = $"STOPPED - Business software detected: {s_settingsManager?.businessSoftwareName}",
                    sizeBytes = 0,
                    transferTimeMs = -1, // Code d'arrêt
                    encryptTimeMs = -1

                };
                s_logger?.Log(stopEntry);
                UpdateStateInFile("BLOCKED");
                return;
            }

            var relativePath = Path.GetRelativePath(sourceDirectory!, file);
            var destinationPath = Path.Combine(targetDirectory!, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            var logEntry = new LoggerLib.LogEntry()
            {
                saveName = name ?? "Unnamed",
                timestamp = DateTime.UtcNow
            };

            // Création des répertoires
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
                var fileInfo = new FileInfo(file);
                var fileSizeBytes = fileInfo.Length;
                bool isLargeFile = BandwidthManager.Instance.IsLargeFile(fileSizeBytes);

                if (isLargeFile)
                {
                    // Gros fichier - essayer d'acquérir le verrou sans attente
                    bool acquired = BandwidthManager.Instance.TryAcquireLargeFileTransfer(
                        file, fileSizeBytes, name ?? "Unnamed", timeout: 0);

                    if (!acquired)
                    {
                        // Reporter ce gros fichier pour plus tard
                        deferredLargeFiles.Add(file);
                        continue;
                    }
                }

                // Copier le fichier
                bool copySuccess = ProcessFileTransfer(file, destinationPath, logEntry, isLargeFile);

                if (!copySuccess && isLargeFile)
                {
                    // Échec - remettre dans la liste des reportés
                    deferredLargeFiles.Add(file);
                }
                if (copySuccess)
                {
                 if (IsEncrypted)
                    {
                        var startEncTime = DateTime.UtcNow;
                        s_cryptoSoftManager?.UseCryptoSoftWithFile(destinationPath, "encode");
                        var endEncTime = DateTime.UtcNow;
                        logEntry.encryptTimeMs = (endEncTime - startEncTime).TotalMilliseconds;
                    }
                    else
                    {
                        logEntry.encryptTimeMs = 0;
                    }
                }
            }

            s_logger?.Log(logEntry);
            filesRemaining--;
            int progression = (int)(((double)(totalFiles - filesRemaining) / totalFiles) * 100);

            // Mise à jour progression
            if (_pauseEvent.WaitOne(0) && !_cts.Token.IsCancellationRequested)
            {
                var state = new SaveState
                {
                    name = name ?? "Unnamed",
                    sourceFilePath = file,
                    targetFilePath = destinationPath,
                    state = "ACTIVE",
                    totalFilesToCopy = totalFiles,
                    totalFilesSize = totalSize,
                    nbFilesLeftToDo = filesRemaining + deferredLargeFiles.Count,
                    progression = Math.Max(0, (int)(((double)(totalFiles - filesRemaining - deferredLargeFiles.Count) / totalFiles) * 100))
                };
                UpdateRealtimeState(state);
            }
        }

        // Deuxième passe : traiter les gros fichiers reportés
        ProcessDeferredLargeFiles(deferredLargeFiles, totalFiles, totalSize).Wait();

        // Finalisation
        if (!_cts.Token.IsCancellationRequested)
        {
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
    }

    private bool ProcessFileTransfer(string file, string destinationPath, LoggerLib.LogEntry logEntry, bool isLargeFile)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            File.Copy(file, destinationPath, true);
            var endTime = DateTime.UtcNow;
            logEntry.transferTimeMs = (endTime - startTime).TotalMilliseconds;
            return true;
        }
        catch
        {
            logEntry.transferTimeMs = -1;
            return false;
        }
        finally
        {
            if (isLargeFile)
            {
                BandwidthManager.Instance.ReleaseLargeFileTransfer(file);
            }
        }
    }

    private async Task ProcessDeferredLargeFiles(List<string> deferredFiles, int totalFiles, long totalSize)
    {
        if (deferredFiles.Count == 0) return;

        foreach (var file in deferredFiles.ToList())
        {
            if (_cts.Token.IsCancellationRequested) return;
            _pauseEvent.WaitOne();
            if (_cts.Token.IsCancellationRequested) return;

            var fileInfo = new FileInfo(file);
            var fileSizeBytes = fileInfo.Length;
            var relativePath = Path.GetRelativePath(sourceDirectory!, file);
            var destinationPath = Path.Combine(targetDirectory!, relativePath);

            // Acquérir le verrou avec timeout long
            bool acquired = await BandwidthManager.Instance.TryAcquireLargeFileTransferAsync(
                file, fileSizeBytes, name ?? "Unnamed", timeout: 60000);

            if (acquired)
            {
                var logEntry = new LoggerLib.LogEntry()
                {
                    saveName = name ?? "Unnamed",
                    timestamp = DateTime.UtcNow,
                    source = file,
                    destination = destinationPath,
                    sizeBytes = fileSizeBytes
                };

                ProcessFileTransfer(file, destinationPath, logEntry, isLargeFile: true);
                s_logger?.Log(logEntry);
                deferredFiles.Remove(file);
            }

            // Mise à jour progression
            int filesRemaining = deferredFiles.Count;
            int progression = Math.Max(0, (int)(((double)(totalFiles - filesRemaining) / totalFiles) * 100));

            if (_pauseEvent.WaitOne(0) && !_cts.Token.IsCancellationRequested)
            {
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
        }
    }

    private void UpdateRealtimeState(SaveState currentState)
    {
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

    // Méthodes de contrôle
    public void Pause()
    {
        try
        {
            _pauseEvent.Reset();
            UpdateStateInFile("PAUSED");
        }
        catch (Exception ex)
        {
            // Log silencieux ou optionnel
        }
    }

    public void Resume()
    {
        try
        {
            _pauseEvent.Set();
            UpdateStateInFile("ACTIVE");
        }
        catch (Exception ex)
        {
            // Log silencieux ou optionnel
        }
    }

    public void Stop()
    {
        try
        {
            _cts.Cancel();
            _pauseEvent.Set();
            UpdateStateInFile("STOPPED");
        }
        catch (Exception ex)
        {
            // Log silencieux ou optionnel
        }
    }

    private void UpdateStateInFile(string newState)
    {
        try
        {
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

            var index = states.FindIndex(s => s.name == this.name);
            if (index >= 0)
            {
                states[index].state = newState;
            }
            else
            {
                states.Add(new SaveState
                {
                    name = this.name ?? "Unnamed",
                    state = newState,
                    totalFilesToCopy = 0,
                    totalFilesSize = 0,
                    nbFilesLeftToDo = 0,
                    progression = 0
                });
            }

            var updatedJson = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(s_stateFilePath, updatedJson);
            SaveState._mutex.ReleaseMutex();
        }
        catch
        {
            // Erreur silencieuse
        }
    }
}
