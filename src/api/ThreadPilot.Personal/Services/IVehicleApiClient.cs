using ThreadPilot.Packages.Models;

namespace ThreadPilot.Personal.Services;

public interface IVehicleApiClient
{
    Task<VehicleInfo?> GetVehicleInfoAsync(string registrationNumber);
}