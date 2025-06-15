using ThreadPilot.Packages.Models;

namespace ThreadPilot.Personal.Services;

public interface IInsuranceService
{
    Task<PersonInsurancesResponse?> GetInsurancesByPersonalIdAsync(string personalId);
}