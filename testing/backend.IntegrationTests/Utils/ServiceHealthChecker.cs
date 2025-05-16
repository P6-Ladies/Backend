// File: Utils/ServiceHealthChecker.cs

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Backend.IntegrationTests.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SkipIfPythonOfflineAttribute : FactAttribute
    {
        private static readonly HttpClient _client = new();
        private const string HealthCheckUrl = "http://localhost:5000/health";

        private static readonly Task<bool> _isAvailable = CheckPythonServiceAsync();

        public SkipIfPythonOfflineAttribute()
        {
            if (!_isAvailable.GetAwaiter().GetResult())
            {
                Skip = $"Skipping test: Python server is not running at {HealthCheckUrl}.";
            }
        }

        private static async Task<bool> CheckPythonServiceAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var response = await _client.GetAsync(HealthCheckUrl, cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}