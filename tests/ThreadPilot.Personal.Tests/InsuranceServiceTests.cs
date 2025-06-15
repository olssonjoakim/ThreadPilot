using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System.Linq;
using ThreadPilot.Personal.Services;
using ThreadPilot.Packages.Models;

namespace ThreadPilot.Personal.Tests;

[TestClass]
public class InsuranceServiceTests
{
    private Mock<IVehicleApiClient> _mockVehicleApiClient;
    private Mock<ILogger<InsuranceService>> _mockLogger;
    private IInsuranceService _insuranceService;

    [TestInitialize]
    public void Setup()
    {
        _mockVehicleApiClient = new Mock<IVehicleApiClient>();
        _mockLogger = new Mock<ILogger<InsuranceService>>();
        _insuranceService = new InsuranceService(_mockVehicleApiClient.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetInsurancesByPersonalIdAsync_ValidPersonalIdWithInsurances_ReturnsInsurances()
    {
        var personalId = "19900101-1234";
        var mockVehicleInfo = new VehicleInfo
        {
            RegistrationNumber = "ABC123",
            Make = "Volvo",
            Model = "XC90",
            Year = 2022,
            Color = "Black"
        };
        
        _mockVehicleApiClient.Setup(x => x.GetVehicleInfoAsync("ABC123"))
            .ReturnsAsync(mockVehicleInfo);
        
        var result = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(personalId, result.PersonalId);
        Assert.AreEqual(2, result.Insurances.Count);
        Assert.AreEqual(50m, result.TotalMonthlyCost);
        
        var carInsurance = result.Insurances.FirstOrDefault(i => i.Type == InsuranceType.Car);
        Assert.IsNotNull(carInsurance);
        Assert.IsNotNull(carInsurance.VehicleInfo);
        Assert.AreEqual("ABC123", carInsurance.VehicleInfo.RegistrationNumber);
    }

    [TestMethod]
    public async Task GetInsurancesByPersonalIdAsync_InvalidPersonalId_ReturnsEmptyResponse()
    {
        var personalId = "00000000-0000";
        
        var result = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(personalId, result.PersonalId);
        Assert.AreEqual(0, result.Insurances.Count);
        Assert.AreEqual(0m, result.TotalMonthlyCost);
    }

    [TestMethod]
    public async Task GetInsurancesByPersonalIdAsync_EmptyPersonalId_ReturnsNull()
    {
        var personalId = "";
        
        var result = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
        
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetInsurancesByPersonalIdAsync_VehicleServiceFails_StillReturnsInsurances()
    {
        var personalId = "19900101-1234";
        
        _mockVehicleApiClient.Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>()))
            .ReturnsAsync((VehicleInfo?)null);
        
        var result = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(personalId, result.PersonalId);
        Assert.AreEqual(2, result.Insurances.Count);
        
        var carInsurance = result.Insurances.FirstOrDefault(i => i.Type == InsuranceType.Car);
        Assert.IsNotNull(carInsurance);
        Assert.IsNull(carInsurance.VehicleInfo);
    }

    [TestMethod]
    public async Task GetInsurancesByPersonalIdAsync_PersonWithAllInsuranceTypes_CalculatesTotalCorrectly()
    {
        var personalId = "19850515-5678";
        
        var result = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(personalId, result.PersonalId);
        Assert.AreEqual(3, result.Insurances.Count);
        Assert.AreEqual(60m, result.TotalMonthlyCost);
        
        Assert.IsTrue(result.Insurances.Any(i => i.Type == InsuranceType.Car && i.MonthlyCost == 30m));
        Assert.IsTrue(result.Insurances.Any(i => i.Type == InsuranceType.Pet && i.MonthlyCost == 10m));
        Assert.IsTrue(result.Insurances.Any(i => i.Type == InsuranceType.PersonalHealth && i.MonthlyCost == 20m));
    }
}