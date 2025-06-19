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
        public ObservableCollection<ServiceStatusModel> Services { get; set; } = new();
        private readonly Timer _timer;

        public MainViewModel()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => LoadStatuses();
            _timer.Start();

            LoadStatuses();
        }

        private void LoadStatuses()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statuses.json");
            if (!File.Exists(path))
                return;

            try
            {
                var json = File.ReadAllText(path);
                var updated = JsonSerializer.Deserialize<ServiceStatusModel[]>(json);

                if (updated == null)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Services.Clear();
                    foreach (var item in updated.OrderBy(x => x.Name))
                        Services.Add(item);
                });
            }
            catch
            {
                // ignore
            }
        }
    }
}