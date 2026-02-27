#!/usr/bin/env bash
# deploy-infra.sh
# Deploys all Azure infrastructure for the AppModAssist project.
#
# Deployment order:
#   1. Resource Group
#   2. Managed Identity   (infra/managed-identity.bicep)
#   3. App Service        (infra/app-service.bicep)
#   4. SQL Database       (infra/sql-database.bicep)
#   5. App Service settings (connection string, AZURE_CLIENT_ID)
#   6. SQL firewall rules (current IP + Azure services)
#
# Usage:
#   bash deploy-infra.sh
#
# Prerequisites:
#   - Azure CLI installed and logged in  (az login)
#   - AdminObjectId and AdminLogin populated in AgentVariables.sh
#   - Sufficient permissions to create resources in the subscription

set -euo pipefail

# ─── Source shared variables ──────────────────────────────────────────────────
source "$(dirname "$0")/AgentVariables.sh"

echo "============================================================"
echo "  AppModAssist – Infrastructure Deployment"
echo "============================================================"
echo "  Resource Group : ${ResourceGroupName}"
echo "  Location       : ${Location}"
echo "============================================================"

# ─── 1. Resource Group ────────────────────────────────────────────────────────
echo ""
echo "[1/6] Creating resource group: ${ResourceGroupName} in ${Location} ..."
az group create \
  --name "${ResourceGroupName}" \
  --location "${Location}" \
  --output table

# ─── 2. Managed Identity ──────────────────────────────────────────────────────
echo ""
echo "[2/6] Deploying managed identity (infra/managed-identity.bicep) ..."
MI_OUTPUT=$(az deployment group create \
  --resource-group "${ResourceGroupName}" \
  --name "deploy-managed-identity" \
  --template-file "$(dirname "$0")/infra/managed-identity.bicep" \
  --parameters location="${Location}" \
  --output json)

ManagedIdentityId=$(echo "${MI_OUTPUT}"       | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['managedIdentityId']['value'])")
ManagedIdentityClientId=$(echo "${MI_OUTPUT}" | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['managedIdentityClientId']['value'])")
ManagedIdentityPrincipalId=$(echo "${MI_OUTPUT}" | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['managedIdentityPrincipalId']['value'])")

echo "  Managed Identity ID          : ${ManagedIdentityId}"
echo "  Managed Identity Client ID   : ${ManagedIdentityClientId}"
echo "  Managed Identity Principal ID: ${ManagedIdentityPrincipalId}"

# ─── 3. App Service ───────────────────────────────────────────────────────────
echo ""
echo "[3/6] Deploying App Service (infra/app-service.bicep) ..."
APP_OUTPUT=$(az deployment group create \
  --resource-group "${ResourceGroupName}" \
  --name "deploy-app-service" \
  --template-file "$(dirname "$0")/infra/app-service.bicep" \
  --parameters \
      location="${Location}" \
      managedIdentityId="${ManagedIdentityId}" \
      managedIdentityClientId="${ManagedIdentityClientId}" \
  --output json)

AppServiceName=$(echo "${APP_OUTPUT}"  | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['appServiceName']['value'])")
AppServiceUrl=$(echo "${APP_OUTPUT}"   | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['appServiceUrl']['value'])")

echo "  App Service Name : ${AppServiceName}"
echo "  App Service URL  : ${AppServiceUrl}"

# ─── 4. SQL Database ──────────────────────────────────────────────────────────
echo ""
echo "[4/6] Deploying SQL Database (infra/sql-database.bicep) ..."

if [[ -z "${AdminObjectId}" || -z "${AdminLogin}" ]]; then
  echo "ERROR: AdminObjectId and AdminLogin must be set in AgentVariables.sh before deploying SQL."
  exit 1
fi

SQL_OUTPUT=$(az deployment group create \
  --resource-group "${ResourceGroupName}" \
  --name "deploy-sql-database" \
  --template-file "$(dirname "$0")/infra/sql-database.bicep" \
  --parameters \
      location="${Location}" \
      adminObjectId="${AdminObjectId}" \
      adminLogin="${AdminLogin}" \
      managedIdentityPrincipalId="${ManagedIdentityPrincipalId}" \
  --output json)

SqlServerName=$(echo "${SQL_OUTPUT}"  | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['sqlServerName']['value'])")
SqlServerFqdn=$(echo "${SQL_OUTPUT}"  | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['sqlServerFqdn']['value'])")
DatabaseName=$(echo "${SQL_OUTPUT}"   | python3 -c "import sys,json; print(json.load(sys.stdin)['properties']['outputs']['databaseName']['value'])")

echo "  SQL Server Name  : ${SqlServerName}"
echo "  SQL Server FQDN  : ${SqlServerFqdn}"
echo "  Database Name    : ${DatabaseName}"

# ─── 5. App Service Settings ──────────────────────────────────────────────────
echo ""
echo "[5/6] Configuring App Service settings ..."

ConnectionString="Server=tcp:${SqlServerFqdn},1433;Database=${DatabaseName};Authentication=Active Directory Managed Identity;User Id=${ManagedIdentityClientId};"

az webapp config connection-string set \
  --resource-group "${ResourceGroupName}" \
  --name "${AppServiceName}" \
  --connection-string-type SQLAzure \
  --settings "DefaultConnection=${ConnectionString}" \
  --output table

az webapp config appsettings set \
  --resource-group "${ResourceGroupName}" \
  --name "${AppServiceName}" \
  --settings \
    "AZURE_CLIENT_ID=${ManagedIdentityClientId}" \
  --output table

echo "  Connection string and AZURE_CLIENT_ID configured."

# ─── 6. SQL Firewall Rules ────────────────────────────────────────────────────
echo ""
echo "[6/6] Configuring SQL Server firewall rules ..."

echo "  Waiting for SQL Server to be fully ready ..."
# Poll until the SQL server is accessible (up to ~5 minutes), then add 30s buffer
az sql server wait \
  --resource-group "${ResourceGroupName}" \
  --name "${SqlServerName}" \
  --exists \
  --timeout 300 || true
echo "  SQL Server is ready. Waiting an additional 30 seconds for firewall propagation ..."
sleep 30

# Allow Azure services (0.0.0.0 – 0.0.0.0 is the Azure services sentinel value)
az sql server firewall-rule create \
  --resource-group "${ResourceGroupName}" \
  --server "${SqlServerName}" \
  --name "AllowAllAzureIPs" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0" \
  --output table

# Allow the current deployment machine's IP
MY_IP=$(curl -s --max-time 10 https://api.ipify.org) || true
if [[ -z "${MY_IP}" ]]; then
  echo "  WARNING: Could not detect deployment machine IP. Skipping AllowDeploymentIP firewall rule."
else
  echo "  Detected deployment machine IP: ${MY_IP}"
  az sql server firewall-rule create \
    --resource-group "${ResourceGroupName}" \
    --server "${SqlServerName}" \
    --name "AllowDeploymentIP" \
    --start-ip-address "${MY_IP}" \
    --end-ip-address "${MY_IP}" \
    --output table
fi

echo "  SQL firewall rules configured."

# ─── Summary ──────────────────────────────────────────────────────────────────
echo ""
echo "============================================================"
echo "  Deployment complete!"
echo "============================================================"
echo "  Resource Group            : ${ResourceGroupName}"
echo "  Managed Identity ID       : ${ManagedIdentityId}"
echo "  Managed Identity Client ID: ${ManagedIdentityClientId}"
echo "  App Service Name          : ${AppServiceName}"
echo "  App Service URL           : ${AppServiceUrl}/Index"
echo "  SQL Server Name           : ${SqlServerName}"
echo "  SQL Server FQDN           : ${SqlServerFqdn}"
echo "  Database Name             : ${DatabaseName}"
echo "============================================================"
echo ""
echo "  NOTE: The application is served at: ${AppServiceUrl}/Index"
echo ""
echo "  To update AgentVariables.sh with deployment outputs, run:"
echo "    sed -i 's|ManagedIdentityId=\"\"|ManagedIdentityId=\"${ManagedIdentityId}\"|' AgentVariables.sh"
echo "    sed -i 's|ManagedIdentityClientId=\"\"|ManagedIdentityClientId=\"${ManagedIdentityClientId}\"|' AgentVariables.sh"
echo "    sed -i 's|ManagedIdentityPrincipalId=\"\"|ManagedIdentityPrincipalId=\"${ManagedIdentityPrincipalId}\"|' AgentVariables.sh"
echo "    sed -i 's|AppServiceUrl=\"\"|AppServiceUrl=\"${AppServiceUrl}\"|' AgentVariables.sh"
echo "    sed -i 's|SqlServerFqdn=\"\"|SqlServerFqdn=\"${SqlServerFqdn}\"|' AgentVariables.sh"
echo "    sed -i 's|ConnectionString=\"\"|ConnectionString=\"${ConnectionString}\"|' AgentVariables.sh"
