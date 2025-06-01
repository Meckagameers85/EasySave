using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Security.Cryptography;

class CryptoSoft
{
    class Settings
    {
        public string[]? extensions { get; set; }
        public string? PrivateKey { get; set; }
        public string? PublicKey { get; set; }
        public string? algorithm { get; set; } = "RSA";
        public string[]? xorkey { get; set; } = new[] { "0x42", "0x13", "0x37", "0x99" };

        public void Save(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(this, options));
        }

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    if (settings != null)
                    {
                        this.extensions = settings.extensions;
                        this.PrivateKey = settings.PrivateKey;
                        this.PublicKey = settings.PublicKey;
                        this.algorithm = settings.algorithm;
                        this.xorkey = settings.xorkey;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading settings: {ex.Message}");
                }
            }
        }
    }

    static Settings settings = new();
    const string settingsPath = "settings.json";
    static string? publicKeyPath;
    static string? privateKeyPath;
    static string[]? extensions;
    static byte[] xorKey = new byte[] { 0x42, 0x13, 0x37, 0x99 };

    static void Main(string[] args)
    {
        settings.Load(settingsPath);

        if (args.Length == 0 || args.Contains("--help"))
        {
            ShowHelp();
            return;
        }

        string? filePath = null;
        string? folderPath = null;
        string? mode = null;
        string? algorithm = null;
        bool settingsModified = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--file": filePath = args[++i]; break;
                case "--folder": folderPath = args[++i]; break;
                case "--mode": mode = args[++i]; break;
                case "--extensions": settings.extensions = args[++i].Split(','); settingsModified = true; break;
                case "--generatekeysRSA": GenerateKeysRSA(); settingsModified = true; break;
                case "--generatekeyXOR": GenerateKeyXOR(); settingsModified = true; break;
                case "--PrivateKey": settings.PrivateKey = args[++i]; settingsModified = true; break;
                case "--PublicKey": settings.PublicKey = args[++i]; settingsModified = true; break;
                case "--algo": algorithm = args[++i]; settings.algorithm = algorithm; settingsModified = true; break;
                case "--xorkey":
                    settings.xorkey = args[++i].Split(',');
                    xorKey = settings.xorkey.Select(hex => Convert.ToByte(hex.Trim().Replace("0x", ""), 16)).ToArray();
                    settingsModified = true;
                    break;
            }
        }

        if (settingsModified)
            settings.Save(settingsPath);

        extensions = settings.extensions;
        publicKeyPath = settings.PublicKey;
        privateKeyPath = settings.PrivateKey;
        algorithm ??= settings.algorithm ?? "RSA";

        if (settings.xorkey != null)
            xorKey = settings.xorkey.Select(hex => Convert.ToByte(hex.Trim().Replace("0x", ""), 16)).ToArray();

        if (!string.IsNullOrEmpty(filePath))
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: The specified path is invalid or the file does not exist.");
                return;
            }

            EnsureValidKeys(algorithm);
            if (string.IsNullOrEmpty(mode) || (mode != "encode" && mode != "decode"))
                Console.WriteLine("Error: please specify a valid mode (--mode encode/decode).");
            else if (extensions == null)
                Console.WriteLine("Error: please specify valid extensions (--extensions ext1,ext2,...).");
            else
                        ProcessFile(filePath, mode, algorithm);
        }
        else if (!string.IsNullOrEmpty(folderPath))
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Error: The specified path is invalid or the folder does not exist.");
                return;
            }
            EnsureValidKeys(algorithm);
            if (string.IsNullOrEmpty(mode) || (mode != "encode" && mode != "decode"))
                Console.WriteLine("Error: please specify a valid mode (--mode encode/decode).");
            else if (extensions == null)
                Console.WriteLine("Error: please specify valid extensions (--extensions ext1,ext2,...).");
            else
                ProcessFolder(folderPath, mode, algorithm);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  CryptoSoft.exe --file <source> --mode <encode/decode> [--algo RSA|xor]");
        Console.WriteLine("  CryptoSoft.exe --folder <source> --mode <encode/decode> [--algo ...]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --extensions <ext1,ext2,...>   Extensions to filter");
        Console.WriteLine("  --algo <RSA|xor>               Encryption method (also in settings.json)");
        Console.WriteLine("  --generatekeysRSA              Generate RSA keys if they do not exist");
        Console.WriteLine("  --PublicKey <path>             Path to the public key (.pub.pem)");
        Console.WriteLine("  --PrivateKey <path>            Path to the private key (.priv.pem)");
        Console.WriteLine("  --generatekeyXOR               Generate a default XOR key if it does not exist");
        Console.WriteLine("  --xorkey <0xAA,0xBB,...>       Custom XOR key (also in settings.json)");
        Console.WriteLine("  --help                         Show this help");
    }

    static void EnsureValidKeys(string algorithm)
    {
        if  (algorithm == "RSA")
        {
            EnsureValidKeysRSA();
        }
        else if (algorithm == "xor")
        {
            EnsureValidKeyXOR();
        }
    }

    static void EnsureValidKeyXOR()
    {
        if (settings.xorkey == null || settings.xorkey.Length == 0)
        {
            GenerateKeyXOR();
        }
        else
        {
            xorKey = settings.xorkey.Select(hex => Convert.ToByte(hex.Trim().Replace("0x", ""), 16)).ToArray();
            Console.WriteLine("Using existing XOR key: " + string.Join(", ", settings.xorkey));
        }
    }
    
    static void GenerateKeyXOR()
    {
        // Generate a random XOR key of 4 bytes
        var rng = RandomNumberGenerator.Create();
        byte[] randomKey = new byte[4];
        rng.GetBytes(randomKey);
        settings.xorkey = randomKey.Select(b => $"0x{b:X2}").ToArray();
        xorKey = settings.xorkey.Select(hex => Convert.ToByte(hex.Trim().Replace("0x", ""), 16)).ToArray();
        settings.Save(settingsPath);
        Console.WriteLine("Default XOR key generated: " + string.Join(", ", settings.xorkey!));
    }
    
    static void EnsureValidKeysRSA()
    {
        bool keysGenerated = false;

        if (string.IsNullOrWhiteSpace(settings.PublicKey) || !settings.PublicKey.EndsWith(".pub.pem"))
        {
            settings.PublicKey = Path.Combine(AppContext.BaseDirectory, "rsa_key.pub.pem");
            keysGenerated = true;
        }

        if (string.IsNullOrWhiteSpace(settings.PrivateKey) || !settings.PrivateKey.EndsWith(".priv.pem"))
        {
            settings.PrivateKey = Path.Combine(AppContext.BaseDirectory, "rsa_key.priv.pem");
            keysGenerated = true;
        }

        if (keysGenerated || !File.Exists(settings.PublicKey) || !File.Exists(settings.PrivateKey))
        {
            GenerateKeysRSA();
        }
        else
        {
            Console.WriteLine("RSA keys already exist.");
        }
    }

    static void GenerateKeysRSA()
    {
        using var rsa = RSA.Create(2048);

        // Définir les chemins à la racine du dossier d'exécution
        string publicKeyPath = Path.Combine(AppContext.BaseDirectory, "rsa_key.pub.pem");
        string privateKeyPath = Path.Combine(AppContext.BaseDirectory, "rsa_key.priv.pem");

        // Écriture des clés
        File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());
        File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());

        // Mise à jour des settings
        settings.PublicKey = publicKeyPath;
        settings.PrivateKey = privateKeyPath;
        settings.Save(settingsPath);

        Console.WriteLine("RSA keys generated at project root.");
    }


    static void ProcessFile(string path, string? mode, string algorithm)
    {
        if (mode == "encode")
            {
                if (extensions != null && !(extensions.Contains(".*") || extensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))))
                {
                    Console.WriteLine($"Skipped (extension not allowed): {path}");
                    return;
                }
                if (algorithm == "xor")
                {
                    if (path.EndsWith(".xor", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped (already XOR encrypted): {path}");
                        return;
                    }
                    EncryptXOR(path);
                }
                else if (algorithm == "RSA")
                {
                    if (path.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped (already RSA encrypted): {path}");
                        return;
                    }
                    EncryptRSA(path);
                }
                else
                {
                    Console.WriteLine($"Unknown algorithm: {algorithm}");
                }
            }
            else if (mode == "decode")
            {
                if (algorithm == "xor")
                {
                    if (!path.EndsWith(".xor", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped (not XOR encrypted): {path}");
                        return;
                    }
                    DecryptXOR(path);
                }
                else if (algorithm == "RSA")
                {
                    if (!path.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped (not RSA encrypted): {path}");
                        return;
                    }
                    DecryptRSA(path);
                }
                else
                {
                    Console.WriteLine($"Unknown algorithm: {algorithm}");
                }
            }
            else
            {
                Console.WriteLine("Error: please specify a valid mode (encode/decode).");
            }
    }

    static void ProcessFolder(string path, string? mode, string algorithm)
    {
        foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
        {
            ProcessFile(file, mode, algorithm);
        }
    }

    static void EncryptRSA(string path)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(publicKeyPath!));
        byte[] encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA1);

        string outputPath = path + ".enc";

        using var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(outputStream);

        writer.Write(encryptedKey.Length);
        writer.Write(encryptedKey);
        writer.Write(aes.IV);
        writer.Flush();

        using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        byte[] buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            cryptoStream.Write(buffer, 0, bytesRead);
        }

        cryptoStream.FlushFinalBlock();
        cryptoStream.Close();

        inputStream.Close();
        File.Delete(path);

        Console.WriteLine($"RSA Encrypted file: {outputPath}");
    }


    static void DecryptRSA(string path)
    {
        string output = path.EndsWith(".enc") ? path.Substring(0, path.Length - 4) : path;

        byte[] encryptedKey;
        byte[] iv;
        byte[] cipherData;

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs))
        {
            int keyLength = br.ReadInt32();
            encryptedKey = br.ReadBytes(keyLength);
            iv = br.ReadBytes(16);
            cipherData = br.ReadBytes((int)(fs.Length - 4 - keyLength - 16));
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPath!));
        byte[] aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA1);

        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = iv;

        byte[] decryptedData;
        using (var msOut = new MemoryStream())
        {
            using (var cs = new CryptoStream(msOut, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherData);
            }
            decryptedData = msOut.ToArray();
        }

        File.WriteAllBytes(output, decryptedData);
        File.Delete(path);
        Console.WriteLine($"RSA Decrypted file: {output}");
    }

    static void EncryptXOR(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        for (int i = 0; i < data.Length; i++)
            data[i] ^= xorKey[i % xorKey.Length];
        File.WriteAllBytes(path + ".xor", data);
        File.Delete(path);
        Console.WriteLine($"XOR encrypted file: {path}.xor");
    }

    static void DecryptXOR(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        for (int i = 0; i < data.Length; i++)
            data[i] ^= xorKey[i % xorKey.Length];
        string output = path.EndsWith(".xor") ? path[..^4] : path;
        File.WriteAllBytes(output, data);
        File.Delete(path);
        Console.WriteLine($"XOR decrypted file: {output}");
    }
}