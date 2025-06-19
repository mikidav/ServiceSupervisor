using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using DroneServiceSupervisor.Utils;

namespace DroneServiceSupervisor.Services
{
    public class HeartbeatReceivedEventArgs : EventArgs
    {
        public string ServiceName { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class HeartbeatListener
    {
        private readonly UdpClient _udpClient;
        private readonly int _port;

        public event EventHandler<HeartbeatReceivedEventArgs> HeartbeatReceived;

        public HeartbeatListener(int port = 9999)
        {
            _port = port;
            _udpClient = new UdpClient(_port);
        }

        public void StartListening()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync();
                        string message = Encoding.UTF8.GetString(result.Buffer);
                        var parts = message.Split(':');

                        if (parts.Length == 3 && parts[0] == "health")
                        {
                            HeartbeatReceived?.Invoke(this, new HeartbeatReceivedEventArgs
                            {
                                ServiceName = parts[1],
                                Status = parts[2],
                                Timestamp = DateTime.Now
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Error receiving heartbeat: " + ex.Message);
                    }
                }
            });
        }
    }
}