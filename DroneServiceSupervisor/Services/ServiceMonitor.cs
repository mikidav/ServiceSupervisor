using DroneServiceSupervisor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;

using DroneServiceSupervisor.Utils;

namespace DroneServiceSupervisor.Services
{
    public class ServiceMonitor
    {
        private readonly Dictionary<string, ServiceConfig> _serviceConfigs;
        private readonly Dictionary<string, DateTime> _lastHeartbeats;
        private readonly Dictionary<string, Process> _runningProcesses;
        private readonly HealthHttpChecker _httpChecker;
        private readonly ServiceStatusWriter _statusWriter;
        private readonly Dictionary<string, ServiceStatus> _statusMap;
        private readonly System.Timers.Timer _checkTimer;

        public ServiceMonitor(List<ServiceConfig> configs)
        {
            _serviceConfigs = configs.ToDictionary(cfg => cfg.Name, cfg => cfg);
            _lastHeartbeats = new Dictionary<string, DateTime>();
            _runningProcesses = new Dictionary<string, Process>();
            _httpChecker = new HealthHttpChecker();
            _statusWriter = new ServiceStatusWriter();
            _statusMap = new Dictionary<string, ServiceStatus>();

            _checkTimer = new System.Timers.Timer(1000); // every 1 sec
            _checkTimer.Elapsed += async (s, e) => await CheckServices();
        }

        public void Start()
        {
            foreach (var service in _serviceConfigs.Values)
            {
                StartService(service);
            }

            _checkTimer.Start();
            _ = PeriodicWriteAsync();
        }

        public void UpdateHeartbeat(string serviceName)
        {
            _lastHeartbeats[serviceName] = DateTime.Now;
        }

        private async Task CheckServices()
        {
            var now = DateTime.Now;

            foreach (var kvp in _serviceConfigs)
            {
                if (!_statusMap.ContainsKey(kvp.Key))
                    _statusMap[kvp.Key] = new ServiceStatus { Status = "Unknown", LastUpdated = DateTime.MinValue };
                {
                    var name = kvp.Key;
                    var config = kvp.Value;

                    _statusMap[name].LastUpdated = DateTime.Now;
                    continue;

                    bool shouldRestart = false;

                    if (config.HealthType == "UDP")
                    {
                        _statusMap[name].Status = "Healthy";
                        if (_lastHeartbeats.TryGetValue(name, out DateTime lastSeen))
                        {
                            if ((now - lastSeen).TotalSeconds > 2)
                            {
                                Logger.Info($"[WARN] No heartbeat from {name}, restarting...");
                                shouldRestart = true;
                            }
                        }
                        else
                        {
                            Logger.Info($"[INIT] No heartbeat yet from {name}");
                        }
                    }
                    else if (config.HealthType == "HTTP")
                    {
                        var result = await _httpChecker.CheckHealthAsync(config);
                        Logger.Info($"[HTTP] {name} health check: {result}");
                        _statusMap[name].Status = result;
                        if (result != "Healthy")
                        {
                            shouldRestart = true;
                        }
                    }

                    if (shouldRestart)
                    {
                        RestartService(config);
                    }
                }
            }

            private void StartService(ServiceConfig config)
            {
                try
                {
                    Thread.Sleep(config.StartupDelay);

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = config.Path,
                        Arguments = config.Args,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        _runningProcesses[config.Name] = process;
                        Logger.Info($"[INFO] Started {config.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info($"[ERROR] Failed to start {config.Name}: {ex.Message}");
                }
            }

            private void RestartService(ServiceConfig config)
            {
                try
                {
                    if (_runningProcesses.TryGetValue(config.Name, out Process proc))
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                    }
                }
                catch (Exception)
                {
                }

                StartService(config);
            }
        }
    }

    private async Task PeriodicWriteAsync()
    {
        while (true)
        {
            await _statusWriter.WriteAsync(_statusMap);
            await Task.Delay(1000);
        }
    }


    public bool RestartService(string name)
    {
        if (!_serviceConfigs.ContainsKey(name))
            return false;

        Logger.Info($">>> Restart request received for {name}");
        StartService(name, _serviceConfigs[name]);
        return true;
    }
}