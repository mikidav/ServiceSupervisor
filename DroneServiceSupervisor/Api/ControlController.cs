using Microsoft.AspNetCore.Mvc;
using DroneServiceSupervisor.Services;
using System.Threading.Tasks;

namespace DroneServiceSupervisor.Api
{
    [ApiController]
    [Route("[controller]")]
    public class ControlController : ControllerBase
    {
        private readonly ServiceMonitor _monitor;

        public ControlController(ServiceMonitor monitor)
        {
            _monitor = monitor;
        }

        [HttpPost("restart")]
        public IActionResult Restart([FromQuery] string service)
        {
            if (string.IsNullOrWhiteSpace(service))
                return BadRequest("Service name required");

            var success = _monitor.RestartService(service);
            return success ? Ok($"Restarting {service}") : NotFound($"Service {service} not found");
        }
    }
}