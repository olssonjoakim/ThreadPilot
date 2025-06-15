using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ThreadPilot.Personal.Services;

namespace ThreadPilot.Personal.Functions;

public class InsuranceFunction
{
    private readonly ILogger<InsuranceFunction> _logger;
    private readonly IInsuranceService _insuranceService;

    public InsuranceFunction(ILogger<InsuranceFunction> logger, IInsuranceService insuranceService)
    {
        _logger = logger;
        _insuranceService = insuranceService;
    }

    [Function("GetInsurances")]
    public async Task<IActionResult> GetInsurances(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "insurances/{personalId}")] HttpRequest req,
        string personalId)
    {
        _logger.LogInformation("Getting insurances for personal ID: {PersonalId}", personalId);

        if (string.IsNullOrWhiteSpace(personalId))
        {
            return new BadRequestObjectResult(new { error = "Personal ID is required" });
        }

        if (!IsValidPersonalId(personalId))
        {
            return new BadRequestObjectResult(new { error = "Invalid personal ID format" });
        }

        try
        {
            var insurances = await _insuranceService.GetInsurancesByPersonalIdAsync(personalId);
            
            if (insurances == null)
            {
                return new NotFoundObjectResult(new { error = $"No insurances found for personal ID '{personalId}'" });
            }

            return new OkObjectResult(insurances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving insurance information");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private bool IsValidPersonalId(string personalId)
    {
        if (string.IsNullOrWhiteSpace(personalId))
            return false;

        var pattern = @"^\d{8}-\d{4}$";
        return System.Text.RegularExpressions.Regex.IsMatch(personalId, pattern);
    }
}