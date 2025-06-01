using System.Text.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EasySaveProject.Models;

public class CryptoSoftManager
{
    private static readonly Lazy<CryptoSoftManager> _instance = new(() => new CryptoSoftManager());
    public static CryptoSoftManager instance => _instance.Value;
    public Mutex mutex = new Mutex(false, "CryptoSoftMutex");
    private readonly object _fileLock = new();
    public string path = "../CryptoSoft";
    private const string CryptoSoftName = "CryptoSoft.exe";
    private const string settingsFile = "settings.json";
    Process cryptoSoftProcess;
    private Dictionary<string, string> _cryptoSoftArguments = new()
    {
        { "SetFile", "--file" },
        { "SetFolder", "--folder" },
        { "SetMode", "--mode" },
        { "SetExtensions", "--extensions" },
        { "SetAlgo", "--algo" },
        { "GetNewRSA", "--generatekeysRSA" },
        { "SetPrivateKeyRSA", "--PrivateKey" },
        { "SetPublicKeyRSA", "--PublicKey" },
        { "GetNewXOR", "--generatekeyXOR" },
        { "SetKeyXOR", "--xorkey" }
    };
    public CryptoSoftSettings Settings { get; set; }

    private CryptoSoftManager()
    {
        cryptoSoftProcess = new Process();
        cryptoSoftProcess.StartInfo.FileName = Path.Combine(path, CryptoSoftName);
        cryptoSoftProcess.StartInfo.WorkingDirectory = path;
        cryptoSoftProcess.StartInfo.CreateNoWindow = true;
        cryptoSoftProcess.StartInfo.UseShellExecute = false;

        Settings = new CryptoSoftSettings();
        LoadSettings();
    }
    public void SetExtensions(List<string> extensions)
    {
        var extensionsString = string.Join(",", extensions);
        var args = $"{_cryptoSoftArguments["SetExtensions"]} {extensionsString}";
        StartCryptoSoft(args);
    }
    public void SetPrivateKeyRSA(string privateKey)
    {
        var args = $"{_cryptoSoftArguments["SetPrivateKeyRSA"]} \"{privateKey}\"";
        StartCryptoSoft(args);
    }
    public void SetPublicKeyRSA(string publicKey)
    {
        var args = $"{_cryptoSoftArguments["SetPublicKeyRSA"]} \"{publicKey}\"";
        StartCryptoSoft(args);
    }
    public void SetAlgorithm(string algorithm)
    {
        var args = $"{_cryptoSoftArguments["SetAlgo"]} {algorithm}";
        StartCryptoSoft(args);
    }
    public void SetXORKey(string key)
    {
        var args = $"{_cryptoSoftArguments["SetKeyXOR"]} {key}";
        StartCryptoSoft(args);
    }
    public void UseCryptoSoftWithFile(string filePath, string mode)
    {
        var args = $"{_cryptoSoftArguments["SetFile"]} \"{filePath}\" {_cryptoSoftArguments["SetMode"]} {mode}";
        StartCryptoSoft(args);
    }
    public void UseCryptoSoftWithFolder(string folderPath, string mode)
    {
        var args = $"{_cryptoSoftArguments["SetFolder"]} \"{folderPath}\" {_cryptoSoftArguments["SetMode"]} {mode}";
        StartCryptoSoft(args);
    }
    private void StartCryptoSoft(string args)
    {
        mutex.WaitOne();
        cryptoSoftProcess.StartInfo.Arguments = args;
        cryptoSoftProcess.Start();
        cryptoSoftProcess.WaitForExit();
        cryptoSoftProcess.StartInfo.Arguments = "";
        mutex.ReleaseMutex();

        LoadSettings();
    }
    public void LoadSettings()
    {
        lock (_fileLock)
        {
            string settingsFilePath = Path.Combine(path, settingsFile);
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                Settings = JsonSerializer.Deserialize<CryptoSoftSettings>(json) ?? new CryptoSoftSettings();
            }
        }
    }
    public class CryptoSoftSettings
    {
        public List<string> extensions { get; set; } = new List<string>();
        public string PrivateKey { get; set; } = "";
        public string PublicKey { get; set; } = "";
        public string algorithm { get; set; } = "RSA";
        public List<string> xorkey { get; set; } = new List<string>();
    }
}

