// File: Utils/ServiceHealthChecker.cs

using System.Net.Http;
using System.Threading.Tasks;

namespace Backend.IntegrationTests.Utils;

public static class ServiceHealthChecker
{
    private static readonly HttpClient client = new();

    public static async Task<bool> IsPythonServiceAvailableAsync()
    {
        try
        {
            var response = await client.GetAsync("http://localhost:5000/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}