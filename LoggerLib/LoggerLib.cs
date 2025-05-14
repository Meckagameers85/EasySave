using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace LoggerLib
{
    public class Logger
    {
        private readonly string logDirectory;
        private static readonly object fileLock = new();

        public Logger(string logDirectory)
        {
            this.logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        public void Log(LogEntry entry)
        {
            string filePath = Path.Combine(logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.json");

            lock (fileLock)
            {
                List<LogEntry> entries = new();

                if (File.Exists(filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        entries = JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
                    }
                    catch
                    {
                        entries = new List<LogEntry>();
                    }
                }

                entries.Add(entry);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(entries, options));
            }
        }
    }

}