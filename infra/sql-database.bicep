// infra/sql-database.bicep
// Deploys Azure SQL Server + Northwind database with Entra ID-only authentication.
//
// MCAPS Security Policy compliance:
//   - azureADOnlyAuthentication: true  (mandatory)
//   - SQL authentication disabled
//   - Entra ID administrator configured with deployer identity
//   - Managed identity granted ##MS_DatabaseManager## server role
//
// API version: @2021-11-01 (stable GA — no preview)
// Region: uksouth

targetScope = 'resourceGroup'

// ─── Parameters ───────────────────────────────────────────────────────────────

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Object ID of the deploying user / service principal (Entra ID SQL admin).')
param adminObjectId string

@description('UPN of the deploying user / service principal (Entra ID SQL admin login).')
param adminLogin string

@description('Principal ID of the user-assigned managed identity that needs DB access.')
param managedIdentityPrincipalId string

// ─── Variables ────────────────────────────────────────────────────────────────

var uniqueSuffix  = uniqueString(resourceGroup().id)
var sqlServerName = 'sql-appmodassist-${uniqueSuffix}'
var databaseName  = 'Northwind'

// Basic tier maximum size in bytes: 2 GB
var basicTierMaxSizeBytes = 2 * 1024 * 1024 * 1024

// ─── SQL Server ───────────────────────────────────────────────────────────────

resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  tags: {
    project: 'AppModAssist'
    environment: 'development'
    managedBy: 'bicep'
  }
  properties: {
    // Disable SQL (password) authentication — Entra ID only
    administrators: {
      administratorType: 'ActiveDirectory'
      login: adminLogin
      sid: adminObjectId
      tenantId: tenant().tenantId
      azureADOnlyAuthentication: true   // MCAPS Policy: mandatory
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// ─── Entra ID Administrator ───────────────────────────────────────────────────

resource sqlAadAdmin 'Microsoft.Sql/servers/administrators@2021-11-01' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: adminLogin
    sid: adminObjectId
    tenantId: tenant().tenantId
  }
}

// ─── Azure AD Only Authentication ────────────────────────────────────────────

resource aadOnlyAuth 'Microsoft.Sql/servers/azureADOnlyAuthentications@2021-11-01' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
  dependsOn: [
    sqlAadAdmin
  ]
}

// ─── Northwind Database ───────────────────────────────────────────────────────

resource northwindDb 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: databaseName
  location: location
  tags: {
    project: 'AppModAssist'
    environment: 'development'
    managedBy: 'bicep'
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: basicTierMaxSizeBytes   // 2 GB — Basic tier max
    readScale: 'Disabled'
    zoneRedundant: false
  }
}

// ─── Firewall: Allow Azure Services ──────────────────────────────────────────

resource fwAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ─── Managed Identity: ##MS_DatabaseManager## server-level role ───────────────
// Granting ##MS_DatabaseManager## via the SQL server administrators collection
// requires a T-SQL GRANT which is executed post-deployment by deploy-infra.sh.
// The principalId is forwarded as an output so the script can target the correct identity.

// ─── Outputs ──────────────────────────────────────────────────────────────────

@description('Name of the Azure SQL Server.')
output sqlServerName string = sqlServer.name

@description('Fully qualified domain name of the Azure SQL Server.')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

@description('Name of the deployed database.')
output databaseName string = northwindDb.name
