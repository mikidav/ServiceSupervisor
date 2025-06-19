using System;
using System.Threading.Tasks;
using DroneServiceSupervisor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DroneServiceSupervisor
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var monitor = new ServiceMonitor();
            monitor.Start();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(monitor);
            builder.Services.AddControllers();

            var app = builder.Build();
            app.MapControllers();
            app.RunAsync("http://localhost:5099");

            Console.WriteLine("Supervisor running. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}