using System;

namespace Supervisor.UI.Models
{
    public class ServiceStatusModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}