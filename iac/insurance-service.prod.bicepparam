using './insurance-service.bicep'

param environment = 'prod'
param location = 'East US'
param appNamePrefix = 'threadpilot'
param storageAccountType = 'Standard_GRS'
param appInsightsLocation = 'East US'
param vehicleServiceBaseUrl = 'https://threadpilot-vehicle-prod.azurewebsites.net/api'