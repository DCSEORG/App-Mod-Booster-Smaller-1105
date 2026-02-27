// infra/managed-identity.bicep
// Deploys a user-assigned managed identity for use by App Service to authenticate
// to Azure SQL Database via Entra ID (no passwords).
//
// Naming pattern: mid-AppModAssist-[uniqueString(resourceGroup().id)]
// Region: uksouth

targetScope = 'resourceGroup'

// ─── Parameters ───────────────────────────────────────────────────────────────

@description('Azure region for all resources.')
param location string = resourceGroup().location

// ─── Variables ────────────────────────────────────────────────────────────────

var uniqueSuffix = uniqueString(resourceGroup().id)
var managedIdentityName = 'mid-appmodassist-${uniqueSuffix}'

// ─── Resources ────────────────────────────────────────────────────────────────

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: {
    project: 'AppModAssist'
    environment: 'development'
    managedBy: 'bicep'
  }
}

// ─── Outputs ──────────────────────────────────────────────────────────────────

@description('Resource ID of the user-assigned managed identity.')
output managedIdentityId string = managedIdentity.id

@description('Client ID of the user-assigned managed identity (used as AZURE_CLIENT_ID).')
output managedIdentityClientId string = managedIdentity.properties.clientId

@description('Principal ID of the user-assigned managed identity (used for SQL role assignments).')
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
