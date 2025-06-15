using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using ThreadPilot.Vehicle.Functions;
using ThreadPilot.Vehicle.Services;
using ThreadPilot.Packages.Models;

namespace ThreadPilot.Vehicle.Tests;

[TestClass]
public class VehicleFunctionTests
{
    private Mock<IVehicleService> _mockVehicleService;
    private Mock<ILogger<VehicleFunction>> _mockLogger;
    private VehicleFunction _function;

    [TestInitialize]
    public void Setup()
    {
        _mockVehicleService = new Mock<IVehicleService>();
        _mockLogger = new Mock<ILogger<VehicleFunction>>();
        _function = new VehicleFunction(_mockLogger.Object, _mockVehicleService.Object);
    }

    [TestMethod]
    public async Task GetVehicle_ValidRegistrationNumber_ReturnsOkResult()
    {
        var registrationNumber = "ABC123";
        var expectedVehicle = new ThreadPilot.Packages.Models.Vehicle
        {
            RegistrationNumber = registrationNumber,
            Make = "Volvo",
            Model = "XC90",
            Year = 2022,
            Color = "Black",
            OwnerPersonalId = "19900101-1234"
        };

        _mockVehicleService.Setup(x => x.GetVehicleByRegistrationNumberAsync(registrationNumber))
            .ReturnsAsync(expectedVehicle);

        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetVehicle(request, registrationNumber);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedVehicle, okResult.Value);
    }

    [TestMethod]
    public async Task GetVehicle_NonExistentRegistrationNumber_ReturnsNotFound()
    {
        var registrationNumber = "NOTFOUND123";
        
        _mockVehicleService.Setup(x => x.GetVehicleByRegistrationNumberAsync(registrationNumber))
            .ReturnsAsync((ThreadPilot.Packages.Models.Vehicle?)null);

        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetVehicle(request, registrationNumber);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetVehicle_EmptyRegistrationNumber_ReturnsBadRequest()
    {
        var registrationNumber = "";
        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetVehicle(request, registrationNumber);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetVehicle_ServiceThrowsException_ReturnsInternalServerError()
    {
        var registrationNumber = "ABC123";
        
        _mockVehicleService.Setup(x => x.GetVehicleByRegistrationNumberAsync(registrationNumber))
            .ThrowsAsync(new Exception("Database error"));

        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetVehicle(request, registrationNumber);

        Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
        var statusCodeResult = (StatusCodeResult)result;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}