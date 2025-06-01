using System.Collections.Concurrent;
using EasySaveProject.Models;

namespace EasySaveProject.Models;

/// <summary>
/// Gestionnaire global de la bande passante pour limiter les transferts de gros fichiers
/// </summary>
public class BandwidthManager
{
    private static readonly Lazy<BandwidthManager> _instance = new(() => new BandwidthManager());
    public static BandwidthManager Instance => _instance.Value;

    // Verrou pour les gros fichiers (un seul à la fois)
    private readonly SemaphoreSlim _largeFileSemaphore = new(1, 1);

    // Tracking des fichiers en cours de transfert
    private readonly ConcurrentDictionary<string, FileTransferInfo> _activeTransfers = new();

    // Pour les statistiques et le debugging
    private readonly object _statsLock = new();
    private int _totalLargeFilesQueued = 0;
    private int _totalSmallFilesTransferred = 0;

    private BandwidthManager() { }

    /// <summary>
    /// Vérifie si un fichier est volumineux selon le seuil configuré
    /// </summary>
    public bool IsLargeFile(long fileSizeBytes)
    {
        var thresholdBytes = SettingsManager.instance.GetBandwidthThresholdBytes();
        return fileSizeBytes >= thresholdBytes;
    }

    /// <summary>
    /// Tente d'acquérir le verrou pour un gros fichier
    /// </summary>
    /// <param name="filePath">Chemin du fichier</param>
    /// <param name="fileSizeBytes">Taille du fichier</param>
    /// <param name="backupName">Nom de la sauvegarde</param>
    /// <param name="timeout">Timeout en millisecondes (défaut: 30s)</param>
    /// <returns>True si le verrou est acquis, False sinon</returns>
    public async Task<bool> TryAcquireLargeFileTransferAsync(string filePath, long fileSizeBytes, string backupName, int timeout = 30000)
    {
        if (!IsLargeFile(fileSizeBytes))
        {
            // Fichier petit - pas besoin de verrou
            RecordSmallFileTransfer();
            return true;
        }

        // Fichier volumineux - acquérir le verrou
        lock (_statsLock)
        {
            _totalLargeFilesQueued++;
        }

        var transferInfo = new FileTransferInfo
        {
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            BackupName = backupName,
            StartTime = DateTime.UtcNow,
            IsLargeFile = true
        };

        // Essayer d'acquérir le verrou avec timeout
        if (await _largeFileSemaphore.WaitAsync(timeout))
        {
            // Verrou acquis - enregistrer le transfert
            _activeTransfers[filePath] = transferInfo;
            return true;
        }
        else
        {
            // Timeout - impossible d'acquérir le verrou
            return false;
        }
    }

    /// <summary>
    /// Version synchrone pour l'intégration simple
    /// </summary>
    public bool TryAcquireLargeFileTransfer(string filePath, long fileSizeBytes, string backupName, int timeout = 30000)
    {
        return TryAcquireLargeFileTransferAsync(filePath, fileSizeBytes, backupName, timeout).Result;
    }

    /// <summary>
    /// Libère le verrou pour un gros fichier
    /// </summary>
    public void ReleaseLargeFileTransfer(string filePath)
    {
        if (_activeTransfers.TryRemove(filePath, out var transferInfo))
        {
            if (transferInfo.IsLargeFile)
            {
                _largeFileSemaphore.Release();
            }
        }
    }

    /// <summary>
    /// Enregistre le transfert d'un petit fichier (pour les stats)
    /// </summary>
    private void RecordSmallFileTransfer()
    {
        lock (_statsLock)
        {
            _totalSmallFilesTransferred++;
        }
    }

    /// <summary>
    /// Obtient le nombre de gros fichiers en attente
    /// </summary>
    public int GetQueuedLargeFilesCount()
    {
        return Math.Max(0, _largeFileSemaphore.CurrentCount == 0 ? _totalLargeFilesQueued - 1 : 0);
    }

    /// <summary>
    /// Vérifie si un gros fichier est actuellement en cours de transfert
    /// </summary>
    public bool IsLargeFileTransferActive()
    {
        return _largeFileSemaphore.CurrentCount == 0;
    }

    /// <summary>
    /// Obtient des informations sur le transfert actuel de gros fichier
    /// </summary>
    public FileTransferInfo? GetCurrentLargeFileTransfer()
    {
        return _activeTransfers.Values.FirstOrDefault(t => t.IsLargeFile);
    }

    /// <summary>
    /// Obtient des statistiques globales
    /// </summary>
    public BandwidthStats GetStats()
    {
        lock (_statsLock)
        {
            return new BandwidthStats
            {
                TotalLargeFilesQueued = _totalLargeFilesQueued,
                TotalSmallFilesTransferred = _totalSmallFilesTransferred,
                CurrentLargeFileTransfer = GetCurrentLargeFileTransfer(),
                QueuedLargeFilesCount = GetQueuedLargeFilesCount(),
                IsLargeFileTransferActive = IsLargeFileTransferActive()
            };
        }
    }

    /// <summary>
    /// Reset des statistiques (pour les tests)
    /// </summary>
    public void ResetStats()
    {
        lock (_statsLock)
        {
            _totalLargeFilesQueued = 0;
            _totalSmallFilesTransferred = 0;
        }
    }
}

/// <summary>
/// Informations sur un transfert de fichier
/// </summary>
public class FileTransferInfo
{
    public string FilePath { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string BackupName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public bool IsLargeFile { get; set; }

    public TimeSpan Duration => DateTime.UtcNow - StartTime;
    public string FileSizeMB => (FileSizeBytes / (1024.0 * 1024.0)).ToString("F1");
}

/// <summary>
/// Statistiques globales de bande passante
/// </summary>
public class BandwidthStats
{
    public int TotalLargeFilesQueued { get; set; }
    public int TotalSmallFilesTransferred { get; set; }
    public FileTransferInfo? CurrentLargeFileTransfer { get; set; }
    public int QueuedLargeFilesCount { get; set; }
    public bool IsLargeFileTransferActive { get; set; }
}