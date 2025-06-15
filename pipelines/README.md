# Azure DevOps CI/CD Pipeline

This folder contains the Azure DevOps pipeline configuration for the ThreadPilot integration layer solution.

## Overview

The pipeline provides automated build, test, and deployment capabilities for both microservices:
- **ThreadPilot Vehicle Service**
- **ThreadPilot Insurance Service**

## Pipeline Structure

### File: `azure-pipelines.yml`

The pipeline is structured in three main stages:

#### 1. Build Stage
- **Triggers**: All branches (automatic)
- **Agent Pool**: `ubuntu-latest`
- **Actions**:
  - Restore NuGet packages for the entire solution
  - Build all projects in Release configuration
  - Run unit tests for both services (18 total tests)
  - Publish test results with coverage reporting
  - Create deployment artifacts for Function Apps
  - Package Bicep infrastructure templates
  - Publish pipeline artifacts for deployment stages

#### 2. Development Deployment
- **Triggers**: `develop` branch only
- **Dependencies**: Build stage must complete successfully
- **Agent Pool**: `ubuntu-latest`
- **Actions**:
  - Deploy infrastructure using Bicep templates (development environment)
  - Deploy Vehicle Service Function App to Azure
  - Deploy Insurance Service Function App to Azure
  - Verify deployment health

#### 3. Production Deployment
- **Triggers**: `main` branch only
- **Dependencies**: Build stage must complete successfully
- **Agent Pool**: `ubuntu-latest`
- **Actions**:
  - Deploy infrastructure using Bicep templates (production environment)
  - Deploy Vehicle Service Function App to Azure
  - Deploy Insurance Service Function App to Azure
  - Verify deployment health

## Prerequisites

### Azure DevOps Setup

1. **Service Connection**: Create Azure Resource Manager service connection
   - Name: `azure-service-connection`
   - Authentication: Service Principal (recommended)
   - Scope: Subscription level with Contributor permissions

2. **Variable Groups**: Create the following variable groups in Azure DevOps:

#### `threadpilot-dev` (Development Environment)
```yaml
resourceGroupName: 'rg-threadpilot-dev'
subscriptionId: 'your-dev-subscription-id'
location: 'West Europe'
environment: 'dev'
vehicleServiceAppName: 'threadpilot-vehicle-dev'
insuranceServiceAppName: 'threadpilot-insurance-dev'
```

#### `threadpilot-prod` (Production Environment)
```yaml
resourceGroupName: 'rg-threadpilot-prod'
subscriptionId: 'your-prod-subscription-id'
location: 'West Europe'
environment: 'prod'
vehicleServiceAppName: 'threadpilot-vehicle-prod'
insuranceServiceAppName: 'threadpilot-insurance-prod'
```

### Azure Resources

Ensure the following Azure resources exist before running the pipeline:

1. **Resource Groups**:
   - `rg-threadpilot-dev` (Development)
   - `rg-threadpilot-prod` (Production)

2. **Service Principal Permissions**:
   - Contributor role on both resource groups
   - Permission to create and manage Azure Function Apps
   - Permission to deploy ARM/Bicep templates

## Pipeline Features

### ✅ Automated Testing
- Runs all 18 unit tests across both services
- Publishes test results in Azure DevOps
- Generates code coverage reports
- Fails pipeline if any tests fail

### ✅ Infrastructure as Code
- Deploys Azure Function Apps using Bicep templates
- Environment-specific parameter files
- Automatic resource provisioning
- Consistent infrastructure across environments

### ✅ Deployment Automation
- Zero-downtime deployment of Function Apps
- Automatic artifact packaging and deployment
- Environment-specific configuration management
- Deployment verification and health checks

### ✅ Branch-based Deployment Strategy
- **develop** branch → Development environment
- **main** branch → Production environment
- Build runs on all branches for validation

## Usage

### Setting Up the Pipeline

1. **Import Pipeline**:
   - In Azure DevOps, go to Pipelines → New Pipeline
   - Choose "Azure Repos Git" or your repository source
   - Select "Existing Azure Pipelines YAML file"
   - Choose `/pipelines/azure-pipelines.yml`

2. **Configure Variables**:
   - Link the variable groups (`threadpilot-dev`, `threadpilot-prod`)
   - Update service connection name if different
   - Verify subscription IDs and resource group names

3. **Run Pipeline**:
   - Pipeline runs automatically on code commits
   - Manual runs can be triggered from Azure DevOps UI
   - Monitor progress through Azure DevOps pipeline interface

### Manual Deployment

For manual deployment outside the pipeline:

```bash
# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"

# Deploy infrastructure (example for dev environment)
az deployment group create \
  --resource-group rg-threadpilot-dev \
  --template-file ../iac/vehicle-service.bicep \
  --parameters ../iac/vehicle-service.dev.bicepparam

# Deploy Function App (after infrastructure deployment)
func azure functionapp publish threadpilot-vehicle-dev
```

## Monitoring and Troubleshooting

### Pipeline Logs
- Build logs are available in Azure DevOps for each pipeline run
- Test results are published and viewable in the Tests tab
- Deployment logs show detailed Azure resource creation

### Common Issues

1. **Test Failures**:
   - Check test output in Azure DevOps Tests tab
   - Verify all dependencies are properly mocked
   - Ensure test data is correctly configured

2. **Infrastructure Deployment Failures**:
   - Verify Azure service connection permissions
   - Check if resource names are unique globally
   - Validate Bicep template syntax

3. **Function App Deployment Failures**:
   - Ensure Function App exists (created by infrastructure stage)
   - Verify deployment credentials and permissions
   - Check Function App configuration and settings

### Performance Metrics

The pipeline typically completes in:
- **Build Stage**: 3-5 minutes
- **Infrastructure Deployment**: 2-3 minutes per environment
- **Function App Deployment**: 1-2 minutes per service
- **Total Pipeline Time**: 8-12 minutes

## Security Considerations

### Secrets Management
- Use Azure Key Vault for sensitive configuration
- Store connection strings as pipeline variables (marked as secret)
- Avoid hardcoding credentials in YAML files

### Access Control
- Limit pipeline edit permissions to authorized users
- Use branch protection policies for main and develop branches
- Implement approval gates for production deployments

### Audit Trail
- All pipeline runs are logged and auditable
- Git commits are linked to pipeline runs
- Deployment history is maintained in Azure DevOps

## Extensions and Customizations

### Adding New Environments
1. Create new variable group (e.g., `threadpilot-staging`)
2. Add new deployment stage to pipeline YAML
3. Create corresponding Bicep parameter files
4. Update branch triggers as needed

### Adding Code Quality Gates
```yaml
# Add to build stage
- task: SonarCloudPrepare@1
  displayName: 'Prepare SonarCloud analysis'
  
- task: SonarCloudAnalyze@1
  displayName: 'Run SonarCloud analysis'
  
- task: SonarCloudPublish@1
  displayName: 'Publish SonarCloud results'
```

### Adding Integration Tests
```yaml
# Add after unit tests
- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests'
  inputs:
    command: 'test'
    projects: 'tests/**/*IntegrationTests.csproj'
    arguments: '--configuration Release --logger trx'
```