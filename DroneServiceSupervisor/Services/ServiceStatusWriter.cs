using DroneServiceSupervisor.Models;
using DroneServiceSupervisor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DroneServiceSupervisor.Services
{
    public class ServiceStatusWriter
    {
        private readonly string _statusPath = Path.Combine("Shared", "status.json");

        public async Task WriteAsync(Dictionary<string, ServiceStatus> statuses)
        {
            try
            {
                Directory.CreateDirectory("Shared");

                var output = new List<object>();
                foreach (var kvp in statuses)
                {
                    output.Add(new
                    {
                        Name = kvp.Key,
                        Status = kvp.Value.Status,
                        LastUpdated = kvp.Value.LastUpdated
                    });
                }

                var json = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_statusPath, json);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to write status file: {ex.Message}");
            }
        }
    }

    public class ServiceStatus
    {
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}