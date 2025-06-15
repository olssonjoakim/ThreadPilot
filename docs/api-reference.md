# ThreadPilot API Reference

## Base URLs

| Environment | Vehicle Service | Insurance Service |
|-------------|----------------|-------------------|
| Development | `https://threadpilot-vehicle-dev.azurewebsites.net` | `https://threadpilot-insurance-dev.azurewebsites.net` |
| Production | `https://threadpilot-vehicle-prod.azurewebsites.net` | `https://threadpilot-insurance-prod.azurewebsites.net` |
| Local | `http://localhost:7071` | `http://localhost:7072` |

## Authentication

Current implementation uses **Anonymous** authorization for demonstration purposes. 

**Production recommendations**: Implement Azure AD, API keys, or certificate-based authentication.

## Vehicle Service API

### Get Vehicle Information

Retrieve vehicle details by registration number.

**Endpoint**: `GET /api/vehicles/{registrationNumber}`

**Parameters**:
- `registrationNumber` (path, required): Vehicle registration number

**Example Request**:
```http
GET /api/vehicles/ABC123 HTTP/1.1
Host: threadpilot-vehicle-dev.azurewebsites.net
```

**Example Response** (200 OK):
```json
{
  "registrationNumber": "ABC123",
  "make": "Volvo",
  "model": "XC90",
  "year": 2022,
  "color": "Black",
  "ownerPersonalId": "19900101-1234"
}
```

**Error Responses**:
- `400 Bad Request`: Registration number is required
- `404 Not Found`: Vehicle not found
- `500 Internal Server Error`: Server error

**Curl Example**:
```bash
curl -X GET "https://threadpilot-vehicle-dev.azurewebsites.net/api/vehicles/ABC123"
```

## Insurance Service API

### Get Insurance Information

Retrieve all insurance policies for a person, including vehicle details for car insurance.

**Endpoint**: `GET /api/insurances/{personalId}`

**Parameters**:
- `personalId` (path, required): Personal identification number (format: YYYYMMDD-XXXX)

**Example Request**:
```http
GET /api/insurances/19900101-1234 HTTP/1.1
Host: threadpilot-insurance-dev.azurewebsites.net
```

**Example Response** (200 OK):
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

**Error Responses**:
- `400 Bad Request`: Personal ID is required or invalid format
- `404 Not Found`: No insurances found
- `500 Internal Server Error`: Server error

**Curl Example**:
```bash
curl -X GET "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19900101-1234"
```

## Data Models

### Vehicle Model

| Field | Type | Description |
|-------|------|-------------|
| `registrationNumber` | string | Vehicle registration number |
| `make` | string | Vehicle manufacturer |
| `model` | string | Vehicle model |
| `year` | integer | Manufacturing year |
| `color` | string | Vehicle color |
| `ownerPersonalId` | string | Personal ID of the vehicle owner |

### Insurance Response Model

| Field | Type | Description |
|-------|------|-------------|
| `personalId` | string | Person's identification number |
| `totalMonthlyCost` | decimal | Sum of all active insurance monthly costs |
| `insurances` | array | List of insurance policies |

### Insurance Policy Model

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique insurance policy identifier |
| `type` | string | Insurance type: "Pet", "PersonalHealth", or "Car" |
| `monthlyCost` | decimal | Monthly premium cost |
| `startDate` | datetime | Policy start date (ISO 8601) |
| `isActive` | boolean | Whether the policy is currently active |
| `vehicleInfo` | object | Vehicle details (only for car insurance) |

### Vehicle Info Model

| Field | Type | Description |
|-------|------|-------------|
| `registrationNumber` | string | Vehicle registration number |
| `make` | string | Vehicle manufacturer |
| `model` | string | Vehicle model |
| `year` | integer | Manufacturing year |
| `color` | string | Vehicle color |

## Test Data

### Available Vehicles

| Registration | Make | Model | Year | Color | Owner |
|-------------|------|-------|------|-------|-------|
| ABC123 | Volvo | XC90 | 2022 | Black | 19900101-1234 |
| XYZ789 | Tesla | Model 3 | 2023 | White | 19850515-5678 |
| DEF456 | BMW | 320i | 2021 | Blue | 19750225-9012 |

### Available Insurance Policies

| Personal ID | Insurance Types | Monthly Cost | Car Registration |
|-------------|----------------|--------------|-----------------|
| 19900101-1234 | Car, Personal Health | $50.00 | ABC123 |
| 19850515-5678 | Car, Pet, Personal Health | $60.00 | XYZ789 |
| 19750225-9012 | Pet | $10.00 | - |

### Insurance Pricing

| Insurance Type | Monthly Cost |
|---------------|--------------|
| Pet | $10.00 |
| Personal Health | $20.00 |
| Car | $30.00 |

## Status Codes

| Code | Description | When Used |
|------|-------------|-----------|
| 200 | OK | Successful request |
| 400 | Bad Request | Invalid input parameters |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Unexpected server error |

## Rate Limiting

Currently no rate limiting is implemented. For production use, consider implementing:
- Rate limiting per client/API key
- Throttling for expensive operations
- Circuit breaker for service dependencies

## CORS

CORS is not currently configured. For browser-based clients, configure CORS policies in the Azure Function App settings.

## Monitoring

Both services are instrumented with Application Insights for:
- Request/response tracking
- Performance monitoring
- Error tracking
- Custom telemetry

## Example Workflows

### Complete Customer Lookup

1. **Get customer's insurance policies**:
   ```bash
   curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19900101-1234"
   ```

2. **Get detailed vehicle information** (optional, if customer has car insurance):
   ```bash
   curl "https://threadpilot-vehicle-dev.azurewebsites.net/api/vehicles/ABC123"
   ```

### Bulk Operations

**Get multiple customers' data**:
```bash
# Customer 1
curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19900101-1234"

# Customer 2  
curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19850515-5678"

# Customer 3
curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/19750225-9012"
```

### Error Handling Examples

**Invalid vehicle registration**:
```bash
curl "https://threadpilot-vehicle-dev.azurewebsites.net/api/vehicles/INVALID123"
# Returns: 404 Not Found
```

**Invalid personal ID format**:
```bash
curl "https://threadpilot-insurance-dev.azurewebsites.net/api/insurances/invalid-format"
# Returns: 400 Bad Request
```

**Missing parameters**:
```bash
curl "https://threadpilot-vehicle-dev.azurewebsites.net/api/vehicles/"
# Returns: 404 Not Found (route not matched)
```

## SDKs and Client Libraries

Currently, no official SDKs are provided. The APIs are standard REST APIs that can be consumed by any HTTP client.

**Recommended client libraries**:
- **.NET**: `HttpClient`, `RestSharp`
- **JavaScript**: `fetch`, `axios`
- **Python**: `requests`, `httpx`
- **Java**: `OkHttp`, `Spring RestTemplate`

## OpenAPI Specification

*Note: OpenAPI/Swagger documentation is not currently available but is recommended for future implementation.*

Example OpenAPI specification structure:
```yaml
openapi: 3.0.3
info:
  title: ThreadPilot API
  version: 1.0.0
servers:
  - url: https://threadpilot-vehicle-dev.azurewebsites.net
    description: Vehicle Service - Development
  - url: https://threadpilot-insurance-dev.azurewebsites.net
    description: Insurance Service - Development
```