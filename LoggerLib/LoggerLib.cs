using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace LoggerLib
{
    public class Logger
    {
        private readonly string logDirectory;

        public string logFormat { get; set; } = "JSON"; // Default format is JSON
        private static readonly object fileLock = new();

        public Logger(string logDirectory)
        {
            this.logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        public Logger(string logDirectory, string logFormat)
        {
            this.logDirectory = logDirectory;
            this.logFormat = logFormat;
            Directory.CreateDirectory(logDirectory);
        }

        public void Log(LogEntry entry)
        {
            if (logFormat == "JSON")
            {
                LogIntoJson(entry);
            }
            else if (logFormat == "XML")
            {
                LogIntoXml(entry);
            }
            else
            {
                throw new NotSupportedException($"Log format '{logFormat}' is not supported.");
            }
        }

        private void LogIntoJson(LogEntry entry)
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

        private void LogIntoXml(LogEntry entry)
        {
            string filePath = Path.Combine(logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.xml");

            lock (fileLock)
            {
                List<LogEntry> entries = new();

                if (File.Exists(filePath))
                {
                    try
                    {
                        using (FileStream stream = new(filePath, FileMode.Open))
                        {
                            XmlSerializer serializer = new(typeof(List<LogEntry>));
                            entries = (List<LogEntry>)serializer.Deserialize(stream) ?? new List<LogEntry>();
                        }
                    }
                    catch
                    {
                        entries = new List<LogEntry>();
                    }
                }

                entries.Add(entry);

                XmlSerializer xmlSerializer = new(typeof(List<LogEntry>));
                using (FileStream stream = new(filePath, FileMode.Create))
                {
                    xmlSerializer.Serialize(stream, entries);
                }
            }
        }
    }

}