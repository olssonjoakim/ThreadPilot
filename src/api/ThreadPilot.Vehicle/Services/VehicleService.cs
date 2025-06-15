using ThreadPilot.Packages.Models;

namespace ThreadPilot.Vehicle.Services;

public class VehicleService : IVehicleService
{
    private readonly Dictionary<string, ThreadPilot.Packages.Models.Vehicle> _vehicleDatabase = new()
    {
        ["ABC123"] = new ThreadPilot.Packages.Models.Vehicle
        {
            RegistrationNumber = "ABC123",
            Make = "Volvo",
            Model = "XC90",
            Year = 2022,
            Color = "Black",
            OwnerPersonalId = "19900101-1234"
        },
        ["XYZ789"] = new ThreadPilot.Packages.Models.Vehicle
        {
            RegistrationNumber = "XYZ789",
            Make = "Tesla",
            Model = "Model 3",
            Year = 2023,
            Color = "White",
            OwnerPersonalId = "19850515-5678"
        },
        ["DEF456"] = new ThreadPilot.Packages.Models.Vehicle
        {
            RegistrationNumber = "DEF456",
            Make = "BMW",
            Model = "320i",
            Year = 2021,
            Color = "Blue",
            OwnerPersonalId = "19750225-9012"
        }
    };

    public Task<ThreadPilot.Packages.Models.Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            return Task.FromResult<ThreadPilot.Packages.Models.Vehicle?>(null);
        }

        var normalizedRegNumber = registrationNumber.ToUpperInvariant().Replace(" ", "");
        
        _vehicleDatabase.TryGetValue(normalizedRegNumber, out var vehicle);
        return Task.FromResult(vehicle);
    }
}