using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ThreadPilot.Vehicle.Services;

namespace ThreadPilot.Vehicle.Functions;

public class VehicleFunction
{
    private readonly ILogger<VehicleFunction> _logger;
    private readonly IVehicleService _vehicleService;

    public VehicleFunction(ILogger<VehicleFunction> logger, IVehicleService vehicleService)
    {
        _logger = logger;
        _vehicleService = vehicleService;
    }

    [Function("GetVehicle")]
    public async Task<IActionResult> GetVehicle(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vehicles/{registrationNumber}")] HttpRequest req,
        string registrationNumber)
    {
        _logger.LogInformation("Getting vehicle information for registration number: {RegistrationNumber}", registrationNumber);

        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            return new BadRequestObjectResult(new { error = "Registration number is required" });
        }

        try
        {
            var vehicle = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
            
            if (vehicle == null)
            {
                return new NotFoundObjectResult(new { error = $"Vehicle with registration number '{registrationNumber}' not found" });
            }

            return new OkObjectResult(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle information");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}