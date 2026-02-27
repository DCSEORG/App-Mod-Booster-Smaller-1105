#!/usr/bin/env bash
# AgentVariables.sh
# Shared variables for all infra and app deployment agents.
# Source this file in shell scripts: source "$(dirname "$0")/AgentVariables.sh"
# All agents must use these variable names to ensure consistency across agentic runs.

# ─── Azure Subscription & Resource Group ──────────────────────────────────────
# The Azure resource group that contains all project resources.
export ResourceGroupName="rg-AppModAssist"

# Azure region for all resources (UKSOUTH as required by prompts).
export Location="uksouth"

# ─── Managed Identity (prompt-001) ────────────────────────────────────────────
# User-assigned managed identity used by App Service to connect to Azure SQL.
# Naming pattern: mid-appmodassist-[uniqueString(resourceGroup().id)]
# NOTE: The actual deployed resource name is lowercase and includes a unique suffix.
#       This base name is used as a prefix in managed-identity.bicep.
export ManagedIdentityName="mid-AppModAssist"

# Resource ID of the managed identity (populated after infra deployment).
export ManagedIdentityId=""

# Client ID of the managed identity (used as AZURE_CLIENT_ID env var in App Service).
export ManagedIdentityClientId=""

# Principal ID of the managed identity (used for SQL database role assignments).
export ManagedIdentityPrincipalId=""

# ─── App Service (prompt-002) ──────────────────────────────────────────────────
# Name of the Azure App Service hosting the ASP.NET application.
# Naming uses uniqueString(resourceGroup().id) for uniqueness; lower case only.
# NOTE: The actual deployed name will be app-appmodassist-[uniqueSuffix].
#       AppServiceName is updated by deploy-infra.sh after deployment.
export AppServiceName="app-appmodassist"

# Public URL of the App Service (populated after infra deployment).
# Note: the application is served at <AppServiceUrl>/Index
export AppServiceUrl=""

# ─── Azure SQL Database (prompt-003) ──────────────────────────────────────────
# Azure SQL Server name (lower case, unique suffix from uniqueString(resourceGroup().id)).
# NOTE: The actual deployed name will be sql-appmodassist-[uniqueSuffix].
#       SqlServerName is updated by deploy-infra.sh after deployment.
export SqlServerName="sql-appmodassist"

# Fully qualified domain name of the SQL server (populated after infra deployment).
export SqlServerFqdn=""

# Name of the database (fixed as Northwind per project requirements).
export DatabaseName="Northwind"

# ─── Entra ID (Azure AD) Admin for SQL (prompt-003) ───────────────────────────
# Object ID of the deploying user/service principal, used as Entra ID SQL administrator.
export AdminObjectId=""

# UPN (User Principal Name) of the deploying user, used as Entra ID SQL administrator login.
export AdminLogin=""

# ─── Application & Deployment Settings (prompt-007, prompt-009) ───────────────
# .NET target framework for the ASP.NET Razor Pages application.
export DotNetFramework="net8.0"

# Connection string template for App Service (managed identity authentication).
# Populated by deploy-infra.sh after SqlServerFqdn, DatabaseName, and ManagedIdentityClientId are known.
# Format: Server=tcp:<SqlServerFqdn>;Database=<DatabaseName>;Authentication=Active Directory Managed Identity;User Id=<ManagedIdentityClientId>;
export ConnectionString=""
