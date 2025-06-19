ServiceSupervisor – System Overview
The DroneServiceSupervisor is a lightweight service management system designed to monitor, control, and ensure the health and resilience of various drone-related services operating in a closed, resource-constrained Windows environment.

🔍 Purpose
The system supervises multiple microservices responsible for drone telemetry, mission planning, communication, video processing, and more. It ensures:

Real-time health monitoring via heartbeat signals (UDP or HTTP)

Automatic service recovery in case of failure

Decoupled communication between the core service and the user interface

🧩 Components
Supervisor Service (REST API) – A Windows background service (or console) that monitors drone services and exposes a REST interface for status and control.

Supervisor UI (WPF) – A lightweight graphical interface that interacts with the supervisor via HTTP to display live statuses and allow manual restarts.

Drone Services – Individual modules (e.g., telemetry, mission planner, gateway, video) each sending periodic health signals.

Configuration – JSON-based configuration for service paths, ports, health protocols, and restart policies.

📡 Health Monitoring
Each drone-related service sends a heartbeat message every 500ms using one of two protocols:

UDP – For minimal-overhead local signaling (health:TelemetryService:Healthy)

HTTP – ASP.NET services expose /healthz endpoints compliant with HealthCheck standards

🛡️ Resilience Features (via Polly or equivalent logic)
Retry logic

Timeout limits

Circuit breaker pattern

Fallback behaviors

Startup delays and dependency ordering

💾 Example Configuration (services.json)
json
Copy
Edit
[
  {
    "Name": "TelemetryService",
    "Path": "Services/TelemetryService.exe",
    "Args": "",
    "HealthType": "UDP",
    "RestartOnFail": true
  },
  {
    "Name": "MissionPlanner",
    "Path": "Services/MissionPlanner.exe",
    "Args": "",
    "HealthType": "HTTP",
    "HealthUrl": "http://localhost:5080/healthz",
    "RestartOnFail": true
  }
]
💡 Design Considerations
Fully offline environment

Minimal resource usage (CPU, RAM)

No external cloud services

Extensible architecture supporting future UI plugins
