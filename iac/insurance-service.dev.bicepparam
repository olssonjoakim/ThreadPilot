using './insurance-service.bicep'

param environment = 'dev'
param location = 'East US'
param appNamePrefix = 'threadpilot'
param storageAccountType = 'Standard_LRS'
param appInsightsLocation = 'East US'
param vehicleServiceBaseUrl = 'https://threadpilot-vehicle-dev.azurewebsites.net/api'