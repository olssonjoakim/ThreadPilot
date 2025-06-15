# ThreadPilot Infrastructure as Code

This directory contains Bicep templates for deploying the ThreadPilot Azure Function Apps and their supporting infrastructure.

## Structure

```
iac/
├── vehicle-service.bicep           # Vehicle Service infrastructure template
├── insurance-service.bicep         # Insurance Service infrastructure template
├── vehicle-service.dev.bicepparam  # Development parameters for Vehicle Service
├── vehicle-service.prod.bicepparam # Production parameters for Vehicle Service
├── insurance-service.dev.bicepparam # Development parameters for Insurance Service
├── insurance-service.prod.bicepparam # Production parameters for Insurance Service
└── README.md                       # This file
```

## Resources Deployed

Each service template deploys the following Azure resources:

### Vehicle Service (`vehicle-service.bicep`)
- **Function App**: `threadpilot-vehicle-{environment}`
- **App Service Plan**: Consumption plan (Y1 SKU)
- **Storage Account**: For Function App runtime
- **Application Insights**: For monitoring and telemetry

### Insurance Service (`insurance-service.bicep`)
- **Function App**: `threadpilot-insurance-{environment}`
- **App Service Plan**: Consumption plan (Y1 SKU)
- **Storage Account**: For Function App runtime
- **Application Insights**: For monitoring and telemetry
- **Configuration**: Includes Vehicle Service URL for service-to-service communication

## Parameters

### Common Parameters
- `environment`: Environment name (dev, test, prod)
- `location`: Azure region for deployment
- `appNamePrefix`: Prefix for resource names (default: threadpilot)
- `storageAccountType`: Storage account replication type
- `appInsightsLocation`: Application Insights region

### Insurance Service Specific
- `vehicleServiceBaseUrl`: Base URL of the Vehicle Service for integration

## Deployment

### Using Azure CLI

Deploy Vehicle Service:
```bash
# Development
az deployment group create \
  --resource-group rg-threadpilot-dev \
  --template-file vehicle-service.bicep \
  --parameters vehicle-service.dev.bicepparam

# Production
az deployment group create \
  --resource-group rg-threadpilot-prod \
  --template-file vehicle-service.bicep \
  --parameters vehicle-service.prod.bicepparam
```

Deploy Insurance Service:
```bash
# Development
az deployment group create \
  --resource-group rg-threadpilot-dev \
  --template-file insurance-service.bicep \
  --parameters insurance-service.dev.bicepparam

# Production
az deployment group create \
  --resource-group rg-threadpilot-prod \
  --template-file insurance-service.bicep \
  --parameters insurance-service.prod.bicepparam
```

### Using Azure DevOps Pipeline

The infrastructure deployment is automated in the Azure DevOps pipeline (`/pipelines/azure-pipelines.yml`). The pipeline:

1. **Build Stage**: Compiles code, runs tests, and packages artifacts
2. **Infrastructure Stage**: Deploys Bicep templates using Azure Resource Manager tasks
3. **Application Stage**: Deploys Function App code to the provisioned infrastructure

### Deployment Order

**Important**: Deploy services in this order due to dependencies:

1. **Vehicle Service** first (no dependencies)
2. **Insurance Service** second (depends on Vehicle Service URL)

This order is automatically handled in the Azure DevOps pipeline.

## Environment Configuration

### Development Environment
- **Resource Group**: `rg-threadpilot-dev`
- **Storage**: Standard LRS (locally redundant)
- **Vehicle Service URL**: `https://threadpilot-vehicle-dev.azurewebsites.net/api`

### Production Environment
- **Resource Group**: `rg-threadpilot-prod`
- **Storage**: Standard GRS (geo-redundant)
- **Vehicle Service URL**: `https://threadpilot-vehicle-prod.azurewebsites.net/api`

## Security Features

All deployed resources include:
- **HTTPS Only**: Force HTTPS for all communications
- **TLS 1.2 Minimum**: Enforce modern TLS version
- **FTPS Only**: Secure file transfer
- **System-Assigned Managed Identity**: For secure Azure service access
- **Storage Account**: Requires HTTPS and OAuth authentication

## Monitoring

Each Function App is configured with:
- **Application Insights**: For performance monitoring, logging, and telemetry
- **Connection String**: Automatically configured in app settings
- **Instrumentation Key**: Available for custom telemetry

## Outputs

Each template provides the following outputs:
- `functionAppName`: Name of the deployed Function App
- `functionAppUrl`: HTTPS URL of the Function App
- `applicationInsightsInstrumentationKey`: For custom telemetry
- `applicationInsightsConnectionString`: For Application Insights integration

## Customization

To customize for your environment:

1. **Update Parameter Files**: Modify `.bicepparam` files for your specific requirements
2. **Resource Naming**: Change `appNamePrefix` parameter
3. **Regions**: Update `location` parameters for your preferred Azure regions
4. **Storage Redundancy**: Adjust `storageAccountType` based on requirements
5. **Service URLs**: Update Vehicle Service URLs in Insurance Service parameters

## Troubleshooting

### Common Issues

1. **Resource Name Conflicts**: Storage account names must be globally unique
2. **Region Availability**: Ensure all services are available in chosen regions
3. **Service Dependencies**: Deploy Vehicle Service before Insurance Service
4. **Parameter Validation**: Check parameter file syntax for Bicep compatibility

### Validation

Validate templates before deployment:
```bash
az deployment group validate \
  --resource-group rg-threadpilot-dev \
  --template-file vehicle-service.bicep \
  --parameters vehicle-service.dev.bicepparam
```