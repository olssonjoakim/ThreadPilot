using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using ThreadPilot.Personal.Functions;
using ThreadPilot.Packages.Models;
using ThreadPilot.Personal.Services;

namespace ThreadPilot.Personal.Tests;

[TestClass]
public class InsuranceFunctionTests
{
    private Mock<IInsuranceService> _mockInsuranceService;
    private Mock<ILogger<InsuranceFunction>> _mockLogger;
    private InsuranceFunction _function;

    [TestInitialize]
    public void Setup()
    {
        _mockInsuranceService = new Mock<IInsuranceService>();
        _mockLogger = new Mock<ILogger<InsuranceFunction>>();
        _function = new InsuranceFunction(_mockLogger.Object, _mockInsuranceService.Object);
    }

    [TestMethod]
    public async Task GetInsurances_ValidPersonalId_ReturnsOkResult()
    {
        var personalId = "19900101-1234";
        var expectedResponse = new PersonInsurancesResponse
        {
            PersonalId = personalId,
            Insurances = new List<InsuranceResponse>
            {
                new InsuranceResponse 
                { 
                    Id = "INS001", 
                    Type = InsuranceType.Car, 
                    MonthlyCost = 30 
                }
            },
            TotalMonthlyCost = 30
        };

        _mockInsuranceService.Setup(x => x.GetInsurancesByPersonalIdAsync(personalId))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetInsurances(request, personalId);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [TestMethod]
    public async Task GetInsurances_InvalidPersonalIdFormat_ReturnsBadRequest()
    {
        var invalidPersonalId = "12345";
        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetInsurances(request, invalidPersonalId);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.IsNotNull(badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetInsurances_EmptyPersonalId_ReturnsBadRequest()
    {
        var personalId = "";
        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetInsurances(request, personalId);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetInsurances_ServiceThrowsException_ReturnsInternalServerError()
    {
        var personalId = "19900101-1234";
        
        _mockInsuranceService.Setup(x => x.GetInsurancesByPersonalIdAsync(personalId))
            .ThrowsAsync(new Exception("Database error"));

        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var result = await _function.GetInsurances(request, personalId);

        Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
        var statusCodeResult = (StatusCodeResult)result;
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
}