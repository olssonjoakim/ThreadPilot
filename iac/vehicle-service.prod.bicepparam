using './vehicle-service.bicep'

param environment = 'prod'
param location = 'East US'
param appNamePrefix = 'threadpilot'
param storageAccountType = 'Standard_GRS'
param appInsightsLocation = 'East US'