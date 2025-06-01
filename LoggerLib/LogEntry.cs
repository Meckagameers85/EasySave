using System;

namespace LoggerLib
{
    public class LogEntry
    {
        public DateTime timestamp { get; set; }
        public string saveName { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public long sizeBytes { get; set; }
        public double transferTimeMs { get; set; }
        public double encryptTimeMs { get; set; }
    }
}
