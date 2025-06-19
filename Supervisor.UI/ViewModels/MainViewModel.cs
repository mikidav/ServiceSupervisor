using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.IO;
using System.Text.Json;
using System.Linq;
using Supervisor.UI.Models;

namespace Supervisor.UI.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<ServiceStatusModel> Services { get; set; }
        private readonly Timer _timer;

        public MainViewModel()
        {
            Services = new ObservableCollection<ServiceStatusModel>();

            // Dummy data for now, in actual step will pull from shared memory or JSON
            Services.Add(new ServiceStatusModel { Name = "TelemetryService", Status = "Healthy", LastUpdated = DateTime.Now });
            Services.Add(new ServiceStatusModel { Name = "MissionPlanner", Status = "Healthy", LastUpdated = DateTime.Now });

            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => Refresh();
            _timer.Start();
        }

        private void Refresh()
        {
            try
            {
                var path = Path.Combine("..", "Shared", "status.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var list = JsonSerializer.Deserialize<ServiceStatusModel[]>(json);
                    if (list != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Services.Clear();
                            foreach (var item in list.OrderBy(x => x.Name))
                                Services.Add(item);
                        });
                    }
                }
            }
            catch {}
        }
        {
            // Later: Pull from shared service state file or memory
            foreach (var svc in Services)
            {
                svc.LastUpdated = DateTime.Now;
            }
        }
    }
}