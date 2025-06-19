using DroneServiceSupervisor.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DroneServiceSupervisor.Services
{
    public class HealthHttpChecker
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncPolicy<string> _policy;

        public HealthHttpChecker()
        {
            _httpClient = new HttpClient();

            var timeout = Policy.TimeoutAsync<string>(0.3); // 300ms timeout
            var retry = Policy.Handle<Exception>().RetryAsync(2);
            var circuit = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(10));

            _policy = Policy.WrapAsync(timeout, retry, circuit);
        }

        public async Task<string> CheckHealthAsync(ServiceConfig config)
        {
            if (string.IsNullOrEmpty(config.HealthUrl))
                return "Unknown";

            try
            {
                return await _policy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(config.HealthUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return "Healthy";
                    }
                    return "Unhealthy";
                });
            }
            catch (BrokenCircuitException)
            {
                return "CircuitOpen";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}