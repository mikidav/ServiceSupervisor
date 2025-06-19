namespace DroneServiceSupervisor.Models
{
    public class ServiceConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Args { get; set; }
        public string HealthType { get; set; } // UDP or HTTP
        public string HealthUrl { get; set; }
        public bool RestartOnFail { get; set; }
        public int StartupDelay { get; set; } // in milliseconds
    }
}