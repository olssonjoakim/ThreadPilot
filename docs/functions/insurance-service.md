# Insurance Service API Documentation

## Overview

The Insurance Service is a serverless Azure Function that provides comprehensive insurance information lookup functionality. It integrates with the Vehicle Service to provide complete insurance details including vehicle information for car insurance policies.

## Service Information

- **Service Name**: ThreadPilot Insurance Service
- **Technology**: Azure Functions v4 (.NET 9)
- **Runtime**: dotnet-isolated
- **Architecture**: Serverless microservice with service-to-service integration
- **Base URL**: 
  - Development: `https://threadpilot-insurance-dev.azurewebsites.net`
  - Production: `https://threadpilot-insurance-prod.azurewebsites.net`

## Dependencies

- **Vehicle Service**: Integrates with Vehicle Service for car insurance details
- **HTTP Client**: Uses HttpClient for service-to-service communication

## API Endpoints

### Get Insurance Information

Retrieves all insurance policies for a person, including vehicle details for car insurance.

#### Endpoint Details
- **Method**: `GET`
- **Path**: `/api/insurances/{personalId}`
- **Function Name**: `GetInsurances`
- **Authorization Level**: Anonymous

#### Parameters

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| `personalId` | string | Path | Yes | The personal identification number (format: YYYYMMDD-XXXX) |

#### Request Example

```http
GET /api/insurances/19900101-1234 HTTP/1.1
Host: threadpilot-insurance-dev.azurewebsites.net
Content-Type: application/json
```

```bash
# Using curl
curl -X GET "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19900101-1234"
```

#### Response Format

**Success Response (200 OK)**

```json
{
  "personalId": "19900101-1234",
  "totalMonthlyCost": 50.00,
  "insurances": [
    {
      "id": "INS001",
      "type": "Car",
      "monthlyCost": 30.00,
      "startDate": "2023-01-01T00:00:00Z",
      "isActive": true,
      "vehicleInfo": {
        "registrationNumber": "ABC123",
        "make": "Volvo",
        "model": "XC90",
        "year": 2022,
        "color": "Black"
      }
    },
    {
      "id": "INS002",
      "type": "PersonalHealth",
      "monthlyCost": 20.00,
      "startDate": "2022-06-01T00:00:00Z",
      "isActive": true,
      "vehicleInfo": null
    }
  ]
}
```

**Response Schema**

| Field | Type | Description |
|-------|------|-------------|
| `personalId` | string | The person's identification number |
| `totalMonthlyCost` | decimal | Sum of all active insurance monthly costs |
| `insurances` | array | List of insurance policies |

**Insurance Object Schema**

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique insurance policy identifier |
| `type` | string | Insurance type: "Pet", "PersonalHealth", or "Car" |
| `monthlyCost` | decimal | Monthly premium cost |
| `startDate` | datetime | Policy start date (ISO 8601) |
| `isActive` | boolean | Whether the policy is currently active |
| `vehicleInfo` | object | Vehicle details (only for car insurance) |

**Vehicle Info Schema** (Car insurance only)

| Field | Type | Description |
|-------|------|-------------|
| `registrationNumber` | string | Vehicle registration number |
| `make` | string | Vehicle manufacturer |
| `model` | string | Vehicle model |
| `year` | integer | Manufacturing year |
| `color` | string | Vehicle color |

#### Error Responses

**Bad Request (400) - Missing Personal ID**
```json
{
  "error": "Personal ID is required"
}
```

**Bad Request (400) - Invalid Format**
```json
{
  "error": "Invalid personal ID format"
}
```

**Not Found (404)**
```json
{
  "error": "No insurances found for personal ID '00000000-0000'"
}
```

**Internal Server Error (500)**
```json
{
  "error": "An internal server error occurred"
}
```

## Input Validation

The service performs comprehensive validation:

1. **Personal ID Validation**:
   - Must not be null or empty
   - Must match format: `YYYYMMDD-XXXX` (regex: `^\d{8}-\d{4}$`)
   - Example valid formats: `19900101-1234`, `19850515-5678`

2. **Format Examples**:
   - ✅ Valid: `19900101-1234`
   - ✅ Valid: `19850515-5678`
   - ❌ Invalid: `12345`
   - ❌ Invalid: `1990-01-01-1234`
   - ❌ Invalid: `19900101`

## Insurance Types and Pricing

| Insurance Type | Monthly Cost | Description |
|---------------|--------------|-------------|
| Pet | $10.00 | Pet insurance coverage |
| PersonalHealth | $20.00 | Personal health insurance |
| Car | $30.00 | Car insurance (includes vehicle details) |

## Sample Data

The service includes the following test insurance data:

### Person: 19900101-1234
- **Car Insurance**: $30/month (Vehicle: ABC123 - Volvo XC90)
- **Personal Health**: $20/month
- **Total**: $50/month

### Person: 19850515-5678
- **Car Insurance**: $30/month (Vehicle: XYZ789 - Tesla Model 3)
- **Pet Insurance**: $10/month
- **Personal Health**: $20/month
- **Total**: $60/month

### Person: 19750225-9012
- **Pet Insurance**: $10/month
- **Total**: $10/month

## Business Logic

### InsuranceService Class

**Location**: `src/api/ThreadPilot.Personal/Services/InsuranceService.cs`

**Key Features**:
- In-memory insurance data storage
- Vehicle Service integration for car insurance
- Graceful degradation when Vehicle Service is unavailable
- Automatic cost calculation
- Active policy filtering

**Method**: `GetInsurancesByPersonalIdAsync`
```csharp
public async Task<PersonInsurancesResponse?> GetInsurancesByPersonalIdAsync(string personalId)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(personalId)) return null;
    
    // Get insurances for person
    if (!_insuranceDatabase.TryGetValue(personalId, out var insurances))
    {
        return new PersonInsurancesResponse
        {
            PersonalId = personalId,
            Insurances = new List<InsuranceResponse>(),
            TotalMonthlyCost = 0
        };
    }

    // Process each active insurance
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

        // Fetch vehicle info for car insurance
        if (insurance.Type == InsuranceType.Car && 
            !string.IsNullOrEmpty(insurance.VehicleRegistrationNumber))
        {
            try
            {
                var vehicleInfo = await _vehicleApiClient.GetVehicleInfoAsync(
                    insurance.VehicleRegistrationNumber);
                insuranceResponse.VehicleInfo = vehicleInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch vehicle info for {RegistrationNumber}", 
                    insurance.VehicleRegistrationNumber);
                // Continue without vehicle info - graceful degradation
            }
        }

        response.Insurances.Add(insuranceResponse);
    }

    response.TotalMonthlyCost = response.Insurances.Sum(i => i.MonthlyCost);
    return response;
}
```

## Vehicle Service Integration

### VehicleApiClient Class

**Location**: `src/api/ThreadPilot.Personal/Services/VehicleApiClient.cs`

**Integration Features**:
- HTTP client for Vehicle Service communication
- Configurable base URL via app settings
- JSON response parsing
- Error handling and logging
- Graceful failure handling

**Configuration**:
```json
{
  "VehicleServiceBaseUrl": "https://threadpilot-vehicle-dev.azurewebsites.net/api"
}
```

**Method**: `GetVehicleInfoAsync`
```csharp
public async Task<VehicleInfo?> GetVehicleInfoAsync(string registrationNumber)
{
    try
    {
        var response = await _httpClient.GetAsync(
            $"{_vehicleServiceBaseUrl}/vehicles/{registrationNumber}");
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Vehicle not found: {RegistrationNumber}", 
                    registrationNumber);
                return null;
            }
            
            _logger.LogError("Failed to get vehicle info. Status: {StatusCode}", 
                response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        
        return new VehicleInfo
        {
            RegistrationNumber = root.GetProperty("registrationNumber").GetString() ?? "",
            Make = root.GetProperty("make").GetString() ?? "",
            Model = root.GetProperty("model").GetString() ?? "",
            Year = root.GetProperty("year").GetInt32(),
            Color = root.GetProperty("color").GetString() ?? ""
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error calling vehicle service for {RegistrationNumber}", 
            registrationNumber);
        return null;
    }
}
```

## Error Handling

The service implements comprehensive error handling:

1. **Input Validation**: Returns 400 for invalid personal ID format
2. **Service Integration**: Graceful degradation when Vehicle Service fails
3. **Not Found**: Returns 404 when no insurances exist
4. **Exception Handling**: Returns 500 for unexpected errors
5. **Logging**: Detailed logging for all operations and errors

### Graceful Degradation

When the Vehicle Service is unavailable:
- Insurance data is still returned
- Vehicle information is omitted from car insurance
- Error is logged but doesn't fail the entire request
- Service continues to operate normally

## Performance Characteristics

- **Cold Start**: ~2-3 seconds (first request after idle)
- **Warm Start**: ~100-200ms (subsequent requests)
- **Memory Usage**: ~96MB baseline (higher due to HTTP client)
- **Concurrent Requests**: Scales automatically with demand
- **Vehicle Service Calls**: Adds ~50-100ms per car insurance policy

## Monitoring and Observability

### Application Insights Integration

The service is configured with Application Insights for:

- **Request Tracking**: All HTTP requests and responses
- **Dependency Tracking**: Vehicle Service HTTP calls
- **Exception Tracking**: Automatic error capture
- **Performance Counters**: CPU, memory, request metrics
- **Custom Telemetry**: Business-specific logging

### Key Metrics

Monitor these metrics in Application Insights:

- `requests/count`: Total request volume
- `requests/duration`: Response time percentiles
- `requests/failed`: Error rate
- `dependencies/count`: Vehicle Service call volume
- `dependencies/duration`: Vehicle Service response times
- `dependencies/failed`: Vehicle Service failure rate
- `exceptions/count`: Exception frequency

### Health Monitoring

**Basic Health Check**:
```http
GET /api/insurances/19900101-1234
```
Expected: 200 OK with insurance data and vehicle info

**Vehicle Service Integration Check**:
```http
GET /api/insurances/19900101-1234
```
Check response includes `vehicleInfo` object for car insurance

## Security Considerations

### Current Implementation
- **Authorization Level**: Anonymous (for demo purposes)
- **HTTPS Enforcement**: Required in production
- **Input Validation**: Personal ID format validation
- **Service Communication**: HTTPS to Vehicle Service

### Production Recommendations
1. **Authentication**: Implement Azure AD or API key authentication
2. **Authorization**: Add role-based access control
3. **Rate Limiting**: Implement request throttling
4. **PII Protection**: Encrypt/mask personal identification data
5. **Service Authentication**: Secure Vehicle Service communication
6. **CORS Configuration**: Restrict allowed origins
7. **Secrets Management**: Use Azure Key Vault for service URLs and keys

## Deployment

### Infrastructure Requirements
- Azure Function App (Consumption Plan)
- Storage Account (for Function runtime)
- Application Insights (for monitoring)
- Vehicle Service dependency

### Environment Variables
```json
{
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;...",
  "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
  "FUNCTIONS_EXTENSION_VERSION": "~4",
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=...",
  "VehicleServiceBaseUrl": "https://threadpilot-vehicle-dev.azurewebsites.net/api"
}
```

### Deployment Order
1. **Deploy Vehicle Service first** (dependency)
2. **Deploy Insurance Service** (depends on Vehicle Service)

### Deployment Commands
```bash
# Deploy via Azure CLI
func azure functionapp publish threadpilot-insurance-dev

# Deploy via Azure DevOps (automated)
# Uses pipeline in /pipelines/azure-pipelines.yml
```

## Testing

### Unit Tests
Located in: `tests/ThreadPilot.Personal.Tests/`

**Test Coverage**:
- Valid personal ID lookup
- Invalid personal ID format handling
- Empty/null input validation
- Vehicle service integration
- Vehicle service failure scenarios
- Cost calculation
- Multiple insurance types

### Integration Testing
```bash
# Run all tests
dotnet test tests/ThreadPilot.Personal.Tests/

# Run specific test
dotnet test --filter "GetInsurances_ValidPersonalId_ReturnsOkResult"
```

### End-to-End Testing
```bash
# Test full integration flow
curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19900101-1234"

# Expected: Insurance data with vehicle information
```

## Extensibility

### Adding New Insurance Types
1. **Update Enum**: Add new type to `InsuranceType` enum
2. **Update Pricing**: Add pricing logic in `InsuranceService`
3. **Update Tests**: Add test coverage for new type
4. **Update Documentation**: Document new insurance type

### Database Integration
For production implementation:

1. **Repository Pattern**: Implement `IInsuranceRepository`
2. **Entity Framework**: Add EF Core with SQL Server/CosmosDB
3. **Caching**: Add Redis cache for frequently accessed data
4. **Data Validation**: Add comprehensive insurance data validation

### API Versioning
Future API versions can be implemented using route prefixes:
```csharp
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/insurances/{personalId}")]
```

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore" Version="2.0.1" />
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.23.0" />
```

### Project References
```xml
<ProjectReference Include="..\..\ThreadPilot.Packages\ThreadPilot.Packages.csproj" />
```

### External Dependencies
- **Vehicle Service**: Required for car insurance vehicle details
- **HTTP Client**: For service-to-service communication

## Troubleshooting

### Common Issues

1. **Vehicle Service Integration Failures**
   - Check Vehicle Service availability
   - Verify `VehicleServiceBaseUrl` configuration
   - Monitor dependency tracking in Application Insights
   - Service still returns insurance data without vehicle info

2. **Personal ID Format Errors**
   - Ensure format: `YYYYMMDD-XXXX`
   - Check regex pattern: `^\d{8}-\d{4}$`
   - Verify input is not URL encoded

3. **Performance Issues**
   - Monitor Vehicle Service response times
   - Consider caching vehicle data
   - Review concurrent request patterns

### Debugging
```bash
# Local development
func start --port 7072

# View logs
func azure functionapp logstream threadpilot-insurance-dev

# Test Vehicle Service integration
curl "https://threadpilot-vehicle-dev.azurewebsites.net/api/vehicles/ABC123"
```

## Circuit Breaker Pattern (Future Enhancement)

For production, consider implementing circuit breaker pattern for Vehicle Service calls:

```csharp
// Example using Polly library
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

## Support and Maintenance

### Code Location
- **Source**: `src/api/ThreadPilot.Personal/`
- **Tests**: `tests/ThreadPilot.Personal.Tests/`
- **Infrastructure**: `iac/insurance-service.bicep`

### Contact Information
- **Team**: ThreadPilot Integration Team
- **Repository**: [ThreadPilot GitHub Repository]
- **Documentation**: This document and inline code comments

### Related Documentation
- [Vehicle Service Documentation](./vehicle-service.md)
- [Infrastructure Documentation](../iac/README.md)
- [Main Project Documentation](../README.md)