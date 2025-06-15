using ThreadPilot.Packages.Models;
using Microsoft.Extensions.Logging;

namespace ThreadPilot.Personal.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IVehicleApiClient _vehicleApiClient;
    private readonly ILogger<InsuranceService> _logger;
    
    private readonly Dictionary<string, List<Insurance>> _insuranceDatabase = new()
    {
        ["19900101-1234"] = new List<Insurance>
        {
            new Insurance
            {
                Id = "INS001",
                PersonalId = "19900101-1234",
                Type = InsuranceType.Car,
                MonthlyCost = 30,
                StartDate = new DateTime(2023, 1, 1),
                IsActive = true,
                VehicleRegistrationNumber = "ABC123"
            },
            new Insurance
            {
                Id = "INS002",
                PersonalId = "19900101-1234",
                Type = InsuranceType.PersonalHealth,
                MonthlyCost = 20,
                StartDate = new DateTime(2022, 6, 1),
                IsActive = true
            }
        },
        ["19850515-5678"] = new List<Insurance>
        {
            new Insurance
            {
                Id = "INS003",
                PersonalId = "19850515-5678",
                Type = InsuranceType.Car,
                MonthlyCost = 30,
                StartDate = new DateTime(2023, 3, 1),
                IsActive = true,
                VehicleRegistrationNumber = "XYZ789"
            },
            new Insurance
            {
                Id = "INS004",
                PersonalId = "19850515-5678",
                Type = InsuranceType.Pet,
                MonthlyCost = 10,
                StartDate = new DateTime(2023, 5, 1),
                IsActive = true
            },
            new Insurance
            {
                Id = "INS005",
                PersonalId = "19850515-5678",
                Type = InsuranceType.PersonalHealth,
                MonthlyCost = 20,
                StartDate = new DateTime(2022, 1, 1),
                IsActive = true
            }
        },
        ["19750225-9012"] = new List<Insurance>
        {
            new Insurance
            {
                Id = "INS006",
                PersonalId = "19750225-9012",
                Type = InsuranceType.Pet,
                MonthlyCost = 10,
                StartDate = new DateTime(2023, 8, 1),
                IsActive = true
            }
        }
    };

    public InsuranceService(IVehicleApiClient vehicleApiClient, ILogger<InsuranceService> logger)
    {
        _vehicleApiClient = vehicleApiClient;
        _logger = logger;
    }

    public async Task<PersonInsurancesResponse?> GetInsurancesByPersonalIdAsync(string personalId)
    {
        if (string.IsNullOrWhiteSpace(personalId))
        {
            return null;
        }

        if (!_insuranceDatabase.TryGetValue(personalId, out var insurances))
        {
            return new PersonInsurancesResponse
            {
                PersonalId = personalId,
                Insurances = new List<InsuranceResponse>(),
                TotalMonthlyCost = 0
            };
        }

        var response = new PersonInsurancesResponse
        {
            PersonalId = personalId,
            Insurances = new List<InsuranceResponse>()
        };

        foreach (var insurance in insurances.Where(i => i.IsActive))
        {
            var insuranceResponse = new InsuranceResponse
            {
                Id = insurance.Id,
                Type = insurance.Type,
                MonthlyCost = insurance.MonthlyCost,
                StartDate = insurance.StartDate,
                IsActive = insurance.IsActive
            };

            if (insurance.Type == InsuranceType.Car && !string.IsNullOrEmpty(insurance.VehicleRegistrationNumber))
            {
                try
                {
                    var vehicleInfo = await _vehicleApiClient.GetVehicleInfoAsync(insurance.VehicleRegistrationNumber);
                    insuranceResponse.VehicleInfo = vehicleInfo;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch vehicle info for registration: {RegistrationNumber}", 
                        insurance.VehicleRegistrationNumber);
                }
            }

            response.Insurances.Add(insuranceResponse);
        }

        response.TotalMonthlyCost = response.Insurances.Sum(i => i.MonthlyCost);

        return response;
    }
}