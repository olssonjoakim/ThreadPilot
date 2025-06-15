using System.Text.Json;
using ThreadPilot.Packages.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ThreadPilot.Personal.Services;

public class VehicleApiClient : IVehicleApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleApiClient> _logger;
    private readonly string _vehicleServiceBaseUrl;

    public VehicleApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<VehicleApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _vehicleServiceBaseUrl = configuration["VehicleServiceBaseUrl"] ?? "http://localhost:7071/api";
    }

    public async Task<VehicleInfo?> GetVehicleInfoAsync(string registrationNumber)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_vehicleServiceBaseUrl}/vehicles/{registrationNumber}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Vehicle not found for registration number: {RegistrationNumber}", registrationNumber);
                    return null;
                }
                
                _logger.LogError("Failed to get vehicle info. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            
            return new VehicleInfo
            {
                RegistrationNumber = root.GetProperty("registrationNumber").GetString() ?? string.Empty,
                Make = root.GetProperty("make").GetString() ?? string.Empty,
                Model = root.GetProperty("model").GetString() ?? string.Empty,
                Year = root.GetProperty("year").GetInt32(),
                Color = root.GetProperty("color").GetString() ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling vehicle service for registration number: {RegistrationNumber}", registrationNumber);
            return null;
        }
    }
}