using System;
using System.IO;

namespace DroneServiceSupervisor.Utils
{
    public static class Logger
    {
        private static readonly string logFilePath = Path.Combine("Logs", $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        private static readonly object _lock = new();

        static Logger()
        {
            Directory.CreateDirectory("Logs");
            File.AppendAllText(logFilePath, $"=== Log Started: {DateTime.Now} ==={Environment.NewLine}");
        }

        public static void Log(string message)
        {
            lock (_lock)
            {
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }
    }
}