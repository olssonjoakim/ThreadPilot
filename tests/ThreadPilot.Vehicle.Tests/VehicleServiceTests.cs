using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThreadPilot.Vehicle.Services;
using System.Threading.Tasks;

namespace ThreadPilot.Vehicle.Tests;

[TestClass]
public class VehicleServiceTests
{
    private IVehicleService _vehicleService;

    [TestInitialize]
    public void Setup()
    {
        _vehicleService = new VehicleService();
    }

    [TestMethod]
    public async Task GetVehicleByRegistrationNumberAsync_ValidRegistration_ReturnsVehicle()
    {
        var registrationNumber = "ABC123";
        
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("ABC123", result.RegistrationNumber);
        Assert.AreEqual("Volvo", result.Make);
        Assert.AreEqual("XC90", result.Model);
        Assert.AreEqual(2022, result.Year);
        Assert.AreEqual("Black", result.Color);
        Assert.AreEqual("19900101-1234", result.OwnerPersonalId);
    }

    [TestMethod]
    public async Task GetVehicleByRegistrationNumberAsync_InvalidRegistration_ReturnsNull()
    {
        var registrationNumber = "INVALID123";
        
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVehicleByRegistrationNumberAsync_EmptyRegistration_ReturnsNull()
    {
        var registrationNumber = "";
        
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVehicleByRegistrationNumberAsync_CaseInsensitive_ReturnsVehicle()
    {
        var registrationNumber = "abc123";
        
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("ABC123", result.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetVehicleByRegistrationNumberAsync_WithSpaces_ReturnsVehicle()
    {
        var registrationNumber = "ABC 123";
        
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync(registrationNumber);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("ABC123", result.RegistrationNumber);
    }
}