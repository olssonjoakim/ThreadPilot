using ThreadPilot.Packages.Models;

namespace ThreadPilot.Vehicle.Services;

public interface IVehicleService
{
    Task<ThreadPilot.Packages.Models.Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber);
}