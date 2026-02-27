// infra/app-service.bicep
// Deploys an Azure App Service Plan (Standard S1) and App Service in uksouth.
// Assigns the user-assigned managed identity created by managed-identity.bicep.
// All resource names are lower case with a unique suffix.

targetScope = 'resourceGroup'

// ─── Parameters ───────────────────────────────────────────────────────────────

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Resource ID of the user-assigned managed identity.')
param managedIdentityId string

@description('Client ID of the user-assigned managed identity (AZURE_CLIENT_ID).')
param managedIdentityClientId string

// ─── Variables ────────────────────────────────────────────────────────────────

var uniqueSuffix     = uniqueString(resourceGroup().id)
var appServicePlanName = 'plan-appmodassist-${uniqueSuffix}'
var appServiceName     = 'app-appmodassist-${uniqueSuffix}'

// ─── Resources ────────────────────────────────────────────────────────────────

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  tags: {
    project: 'AppModAssist'
    environment: 'development'
    managedBy: 'bicep'
  }
  sku: {
    name: 'S1'
    tier: 'Standard'
    size: 'S1'
    family: 'S'
    capacity: 1
  }
  kind: 'app'
  properties: {
    reserved: false   // Windows host
  }
}

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  tags: {
    project: 'AppModAssist'
    environment: 'development'
    managedBy: 'bicep'
  }
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentityClientId
        }
      ]
    }
  }
}

// ─── Outputs ──────────────────────────────────────────────────────────────────

@description('Name of the deployed App Service.')
output appServiceName string = appService.name

@description('Default hostname URL of the App Service (https).')
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'

@description('Principal ID of the user-assigned managed identity attached to the App Service.')
output managedIdentityPrincipalId string = reference(managedIdentityId, '2023-01-31').principalId

@description('Client ID of the user-assigned managed identity (AZURE_CLIENT_ID).')
output managedIdentityClientId string = managedIdentityClientId
