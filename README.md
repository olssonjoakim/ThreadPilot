# ThreadPilot Integration Layer

A microservices-based integration layer between a new core system (ThreadPilot) and multiple legacy systems, built with Azure Functions and .NET 9.

## Overview

This solution consists of two separate microservices:
1. **Vehicle Service**: Provides vehicle information based on registration numbers
2. **Insurance Service**: Provides insurance information for individuals, integrating with the Vehicle Service for car insurance details

## Architecture and Design Decisions

### Technology Stack
- **Framework**: .NET 9 with Azure Functions v4
- **API Type**: RESTful HTTP APIs
- **Testing**: MSTest with Moq for mocking
- **Architecture Pattern**: Microservices with service-to-service communication

### Key Design Decisions

1. **Azure Functions**: Chosen for serverless architecture, automatic scaling, and cost-effectiveness
2. **Separate Projects**: Each service is a standalone project to ensure isolation and independent deployment
3. **In-Memory Data Storage**: Used for simplicity in this demo; production would use a database
4. **HTTP Client Integration**: Insurance Service uses HttpClient to communicate with Vehicle Service
5. **Dependency Injection**: Used throughout for testability and loose coupling
6. **Async/Await Pattern**: All operations are asynchronous for better performance

### Project Structure
```
ThreadPilot/
├── src/
│   ├── ThreadPilot.Packages/      # Shared models and contracts
│   │   └── Models/               # Common data models
│   ├── api/
│   │   ├── ThreadPilot.Vehicle/   # Vehicle microservice
│   │   │   ├── Functions/         # HTTP endpoints
│   │   │   └── Services/         # Business logic
│   │   ├── ThreadPilot.Personal/  # Insurance microservice
│   │   │   ├── Functions/         # HTTP endpoints
│   │   │   └── Services/         # Business logic
│   │   └── ThreadPilot.sln       # Solution file
├── tests/
│   ├── ThreadPilot.Vehicle.Tests/ # Vehicle service tests
│   └── ThreadPilot.Personal.Tests/# Insurance service tests
├── iac/                          # Infrastructure as Code (Bicep)
│   ├── vehicle-service.bicep     # Vehicle service infrastructure
│   ├── insurance-service.bicep   # Insurance service infrastructure
│   └── *.bicepparam             # Environment-specific parameters
├── requests/                     # HTTP request files for testing
│   ├── vehicle-service.http      # Vehicle Service API tests
│   ├── insurance-service.http    # Insurance Service API tests
│   └── integration-tests.http    # End-to-end integration tests
└── pipelines/
    └── azure-pipelines.yml       # CI/CD pipeline with infrastructure deployment
```

## API Endpoints

### Vehicle Service (Port 7071)
- **GET** `/api/vehicles/{registrationNumber}`
  - Returns vehicle information for the given registration number
  - Example: `GET http://localhost:7071/api/vehicles/ABC123`

### Insurance Service (Port 7072)
- **GET** `/api/insurances/{personalId}`
  - Returns all insurances for a person with monthly costs
  - Includes vehicle information for car insurances
  - Example: `GET http://localhost:7072/api/insurances/19900101-1234`

📖 **For complete API documentation, see [docs/api-reference.md](docs/api-reference.md)**

🧪 **For testing the APIs, see HTTP request files in [requests/](requests/)**

## Running the Solution Locally

### Prerequisites
- .NET 9 SDK
- Azure Functions Core Tools v4
- Visual Studio 2022 or VS Code with C# extension

### Steps to Run

1. Clone the repository
```bash
git clone <repository-url>
cd ThreadPilot
```

2. Build the solution
```bash
dotnet build
```

3. Start the Vehicle Service (in one terminal)
```bash
cd src/ThreadPilot.Vehicle
func start --port 7071
```

4. Start the Insurance Service (in another terminal)
```bash
cd src/ThreadPilot.Personal
func start --port 7072
```

5. Test the endpoints using curl or your preferred API client:
```bash
# Get vehicle information
curl http://localhost:7071/api/vehicles/ABC123

# Get insurance information
curl http://localhost:7072/api/insurances/19900101-1234
```

### Testing with HTTP Files

Use the provided HTTP request files for comprehensive testing in Visual Studio/VS Code:

1. **Vehicle Service Tests**: Open `requests/vehicle-service.http`
2. **Insurance Service Tests**: Open `requests/insurance-service.http`
3. **Integration Tests**: Open `requests/integration-tests.http`

Each file contains pre-configured requests for different scenarios including success cases, error handling, and performance testing.

## Running Tests

Run all tests from the solution root:
```bash
dotnet test
```

Run tests for a specific project:
```bash
dotnet test tests/ThreadPilot.Vehicle.Tests/
dotnet test src/ThreadPilot.Personal.Tests/
```

## Error Handling Approach

1. **Input Validation**: All endpoints validate input parameters
2. **Graceful Degradation**: If vehicle service is unavailable, insurance service still returns insurance data without vehicle details
3. **Appropriate HTTP Status Codes**:
   - 200 OK: Successful response
   - 400 Bad Request: Invalid input
   - 404 Not Found: Resource not found
   - 500 Internal Server Error: Unexpected errors
4. **Structured Error Responses**: Consistent error format with meaningful messages
5. **Logging**: Comprehensive logging for debugging and monitoring

## Extensibility

The solution is designed for easy extension:

1. **New Insurance Types**: Add new enum values to `InsuranceType`
2. **Additional Services**: Create new Azure Function projects following the same pattern
3. **Database Integration**: Replace in-memory storage with repository pattern
4. **Authentication**: Add Azure AD or API key authentication
5. **API Versioning**: Use route prefixes (e.g., `/api/v1/vehicles`)
6. **Message Queue Integration**: Add Service Bus triggers for async processing

## Security Considerations

Current implementation focuses on functionality. For production:
1. Add authentication (Azure AD, API keys)
2. Implement authorization (role-based access)
3. Use HTTPS only
4. Add rate limiting
5. Validate and sanitize all inputs
6. Implement proper CORS policies
7. Store sensitive configuration in Azure Key Vault

## Sample Test Data

### Vehicles
- ABC123: Volvo XC90 (Owner: 19900101-1234)
- XYZ789: Tesla Model 3 (Owner: 19850515-5678)
- DEF456: BMW 320i (Owner: 19750225-9012)

### Insurance Holders
- 19900101-1234: Car + Personal Health insurance
- 19850515-5678: Car + Pet + Personal Health insurance
- 19750225-9012: Pet insurance only

## Future Enhancements

1. **Containerization**: Add Docker support for container deployment
2. **API Gateway**: Implement Azure API Management for centralized management
3. **Caching**: Add Redis cache for frequently accessed data
4. **Health Checks**: Implement health check endpoints
5. **Metrics and Monitoring**: Add Application Insights custom metrics
6. **GraphQL API**: Provide GraphQL endpoint for flexible queries
7. **Event Sourcing**: Implement event-driven architecture for audit trails

## CI/CD Pipeline

A comprehensive Azure DevOps pipeline configuration is included in `/pipelines/azure-pipelines.yml` that:
- **Build Stage**:
  - Builds the solution
  - Runs all tests (18 unit tests)
  - Publishes test results and coverage
  - Creates deployment artifacts
  - Packages Bicep infrastructure templates
- **Development Deployment** (develop branch):
  - Deploys infrastructure using Bicep templates
  - Deploys Function Apps to Azure
- **Production Deployment** (main branch):
  - Deploys infrastructure with production configuration
  - Deploys Function Apps to production environment

## Infrastructure as Code

The solution includes Bicep templates in `/iac/` for:
- **Azure Function Apps**: Serverless compute for both services
- **Storage Accounts**: Required for Function App runtime
- **Application Insights**: Monitoring and telemetry
- **App Service Plans**: Consumption-based scaling
- **Environment-specific parameters**: Separate configs for dev/prod

Deploy infrastructure manually:
```bash
# Deploy Vehicle Service
az deployment group create \
  --resource-group rg-threadpilot-dev \
  --template-file iac/vehicle-service.bicep \
  --parameters iac/vehicle-service.dev.bicepparam

# Deploy Insurance Service  
az deployment group create \
  --resource-group rg-threadpilot-dev \
  --template-file iac/insurance-service.bicep \
  --parameters iac/insurance-service.dev.bicepparam
```

## Documentation

Comprehensive documentation is available in the `/docs` folder:

- **[Documentation Overview](docs/README.md)** - Complete documentation index
- **[API Reference](docs/api-reference.md)** - Quick API reference with examples
- **[Vehicle Service Documentation](docs/functions/vehicle-service.md)** - Detailed Vehicle Service API docs
- **[Insurance Service Documentation](docs/functions/insurance-service.md)** - Detailed Insurance Service API docs
- **[Functions Overview](docs/functions/README.md)** - Azure Functions development guide
- **[Infrastructure Documentation](iac/README.md)** - Bicep templates and deployment guide

## Personal Reflection

During the years I've worked with a few similar systems. 
1. With Absolut Vodka, I was working on a large legacy platform based on containers running in the Cloud. Scaling and performance was an issue, so it was redesigned using Azure resources ( Azure Functions, web app, Cosmos DB etc ). A few of the services needed to communicate with other services so a solution of ServiceBus and Event Grid was implemented.
2. With Espresso House, I have been working on a IAM platform that contains on a number of microservices. For example, this platform contains a aggregator function to call other services to aggregate data and pass along according to business rules. 
3. With Espresso House, I have been working on a Organization platform that supports the complex organization of the business. A graph database was used for relations storage and a number of Azure Functions to manage both relations and the data. The platform contains a number of Azure Function, one for CRUD operations, a integration services that aggregates data to support external integrations and a long running process function to handle long running processes.

Time.

If I had more time, I would:
1. Implement comprehensive integration tests
2. Add OpenAPI/Swagger documentation for better API discovery
3. Implement a circuit breaker pattern for the service-to-service communication
4. Add performance tests to ensure the solution meets SLA requirements
5. Implement Azure Key Vault integration for secure configuration management
6. Add health check endpoints for better monitoring and orchestration