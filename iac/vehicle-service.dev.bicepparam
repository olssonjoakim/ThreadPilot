using './vehicle-service.bicep'

param environment = 'dev'
param location = 'East US'
param appNamePrefix = 'threadpilot'
param storageAccountType = 'Standard_LRS'
param appInsightsLocation = 'East US'