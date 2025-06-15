# ThreadPilot HTTP Request Files

This directory contains HTTP request files (.http) that can be used with Visual Studio, Visual Studio Code, JetBrains IDEs, and other tools that support the HTTP Client format.

## Files Overview

| File | Purpose | Description |
|------|---------|-------------|
| `vehicle-service.http` | Vehicle Service Testing | Complete test suite for Vehicle Service API |
| `insurance-service.http` | Insurance Service Testing | Complete test suite for Insurance Service API |
| `integration-tests.http` | Integration Testing | End-to-end tests for service-to-service communication |
| `README.md` | Documentation | This file |

## Prerequisites

### Development Environment Setup

1. **Local Development**:
   ```bash
   # Terminal 1: Start Vehicle Service
   cd src/api/ThreadPilot.Vehicle
   func start --port 7071

   # Terminal 2: Start Insurance Service  
   cd src/api/ThreadPilot.Personal
   func start --port 7072
   ```

2. **Visual Studio/VS Code**: 
   - Install REST Client extension (VS Code) or use built-in HTTP client (Visual Studio)
   - Open any .http file and run requests

## Environment Configuration

Each HTTP file includes variables for different environments:

### Local Development
- Vehicle Service: `http://localhost:7071`
- Insurance Service: `http://localhost:7072`

### Development Environment
- Vehicle Service: `https://threadpilot-vehicle-dev.azurewebsites.net`
- Insurance Service: `https://threadpilot-insurance-dev.azurewebsites.net`

### Production Environment
- Vehicle Service: `https://threadpilot-vehicle-prod.azurewebsites.net`
- Insurance Service: `https://threadpilot-insurance-prod.azurewebsites.net`

## Quick Start

### 1. Vehicle Service Tests (`vehicle-service.http`)

**Basic Test**:
```http
GET http://localhost:7071/api/vehicles/ABC123
Accept: application/json
```

**Expected Response**:
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

### 2. Insurance Service Tests (`insurance-service.http`)

**Basic Test**:
```http
GET http://localhost:7072/api/insurances/19900101-1234
Accept: application/json
```

**Expected Response**:
```json
{
  "personalId": "19900101-1234",
  "totalMonthlyCost": 50.00,
  "insurances": [
    {
      "id": "INS001",
      "type": "Car",
      "monthlyCost": 30.00,
      "vehicleInfo": {
        "registrationNumber": "ABC123",
        "make": "Volvo",
        "model": "XC90",
        "year": 2022,
        "color": "Black"
      }
    }
  ]
}
```

### 3. Integration Tests (`integration-tests.http`)

Tests the complete workflow between both services to ensure data consistency and proper integration.

## Test Categories

### Vehicle Service Tests

#### ✅ **Successful Requests**
- Valid vehicle registrations (ABC123, XYZ789, DEF456)
- Case-insensitive lookups
- Registration number normalization (spaces)

#### ❌ **Error Scenarios**
- Invalid registration numbers (404 Not Found)
- Malformed URLs
- Empty parameters

#### 🔧 **Edge Cases**
- Special characters in registration
- URL encoding
- Very long registration numbers

#### 📊 **Performance Tests**
- Response time measurement
- Cold start simulation
- Load testing scenarios

### Insurance Service Tests

#### ✅ **Successful Requests**
- Valid personal IDs with different insurance combinations
- Car insurance with vehicle integration
- Pet and health insurance only

#### ❌ **Error Scenarios**
- Invalid personal ID formats
- Non-existent personal IDs
- Malformed personal ID patterns

#### 🔗 **Integration Tests**
- Vehicle Service integration
- Graceful degradation when Vehicle Service fails
- Data consistency validation

#### 📏 **Validation Tests**
- Personal ID format validation (YYYYMMDD-XXXX)
- Input sanitization
- Pattern matching tests

### Integration Tests

#### 🔄 **Workflow Tests**
- Complete customer lookup workflow
- Vehicle-focused lookup workflow
- Cross-service data validation

#### 🛡️ **Error Handling**
- Service dependency failure simulation
- Invalid cross-references
- Network timeout scenarios

#### ⚡ **Performance Tests**
- Integration overhead measurement
- Concurrent request testing
- Service stability under load

## Test Data Reference

### Available Vehicles
| Registration | Make | Model | Year | Color | Owner |
|-------------|------|-------|------|-------|-------|
| ABC123 | Volvo | XC90 | 2022 | Black | 19900101-1234 |
| XYZ789 | Tesla | Model 3 | 2023 | White | 19850515-5678 |
| DEF456 | BMW | 320i | 2021 | Blue | 19750225-9012 |

### Available Insurance Policies
| Personal ID | Insurance Types | Monthly Cost | Vehicle |
|-------------|----------------|--------------|---------|
| 19900101-1234 | Car, Health | $50.00 | ABC123 |
| 19850515-5678 | Car, Pet, Health | $60.00 | XYZ789 |
| 19750225-9012 | Pet | $10.00 | None |

## Using the HTTP Files

### Visual Studio Code
1. Install the "REST Client" extension
2. Open any .http file
3. Click "Send Request" above any request
4. View response in the right panel

### Visual Studio 2022
1. Open any .http file
2. Click the "Run" button next to any request
3. View response in the output window

### JetBrains IDEs (IntelliJ, Rider)
1. Open any .http file
2. Click the green arrow next to any request
3. View response in the HTTP Client tool window

### Command Line (using httpx or curl)
Extract requests manually:
```bash
# Vehicle Service
curl http://localhost:7071/api/vehicles/ABC123

# Insurance Service  
curl http://localhost:7072/api/insurances/19900101-1234
```

## Environment Switching

To switch between environments, modify the base URL variables at the top of each file:

**For Local Testing**:
```http
@baseUrl_local = http://localhost:7071
```

**For Development Environment**:
```http
@baseUrl_dev = https://threadpilot-vehicle-dev.azurewebsites.net
```

**For Production Environment**:
```http
@baseUrl_prod = https://threadpilot-vehicle-prod.azurewebsites.net
```

## Test Scenarios

### 🟢 **Health Checks**
Use these requests to verify service health:
```http
# Vehicle Service Health
GET http://localhost:7071/api/vehicles/ABC123

# Insurance Service Health  
GET http://localhost:7072/api/insurances/19900101-1234

# Integration Health (includes Vehicle Service call)
GET http://localhost:7072/api/insurances/19900101-1234
```

### 🔴 **Error Testing**
Test error scenarios:
```http
# 404 Not Found
GET http://localhost:7071/api/vehicles/INVALID123

# 400 Bad Request
GET http://localhost:7072/api/insurances/invalid-format
```

### 🔄 **Load Testing**
Run multiple requests to test performance:
1. Open `vehicle-service.http`
2. Navigate to "LOAD TESTING" section
3. Run multiple requests consecutively
4. Monitor response times

## Troubleshooting

### Common Issues

1. **Connection Refused**:
   - Ensure services are running locally
   - Check port numbers (7071 for Vehicle, 7072 for Insurance)
   - Verify firewall settings

2. **404 Not Found**:
   - Check service URLs
   - Verify route patterns
   - Ensure services deployed correctly

3. **500 Internal Server Error**:
   - Check service logs
   - Verify dependencies (Insurance → Vehicle Service)
   - Check Application Insights for detailed errors

4. **Timeout Errors**:
   - Check service health
   - Verify network connectivity
   - Monitor service performance metrics

### Service Dependencies

**Important**: Insurance Service depends on Vehicle Service for car insurance data. When testing:

1. **Start Vehicle Service first** (port 7071)
2. **Then start Insurance Service** (port 7072)
3. **Test Vehicle Service independently** first
4. **Test Insurance Service** (will call Vehicle Service automatically)

### Local Development URLs

Ensure your local development uses these URLs:
- Vehicle Service: `http://localhost:7071`
- Insurance Service: `http://localhost:7072`

### Environment-Specific Testing

When testing against deployed environments, ensure:
- Services are deployed and running
- Network connectivity is available
- Authentication is configured (if required)
- CORS is configured for browser clients

## Best Practices

1. **Start with Health Checks**: Always test basic functionality first
2. **Test Error Scenarios**: Verify proper error handling
3. **Validate Integration**: Test service-to-service communication
4. **Performance Testing**: Monitor response times and service stability
5. **Environment Isolation**: Test each environment separately

## Support

For issues with HTTP request files:
- Check service logs and Application Insights
- Verify service health using health check requests
- Review [API Documentation](../docs/api-reference.md)
- Contact the ThreadPilot Integration Team