#!/bin/bash
# AgentVariables.sh
# Runtime variables populated by Booster-Spec-Agent after infrastructure deployment.
# Source this file in deployment scripts: source "$(dirname "$0")/AgentVariables.sh"
# All variables are exported so they are available to Python subprocesses.

# ─── Azure Subscription & Resource Group ──────────────────────────────────────
export ResourceGroupName="rg-AppModAssist"
export Location="uksouth"

# ─── Managed Identity ─────────────────────────────────────────────────────────
export ManagedIdentityName="mid-AppModAssist"
export ManagedIdentityId=""
export ManagedIdentityClientId=""
export ManagedIdentityPrincipalId=""

# ─── App Service ──────────────────────────────────────────────────────────────
export AppServiceName="app-appmodassist"
export AppServiceUrl=""

# ─── Azure SQL Database ───────────────────────────────────────────────────────
export SqlServerName="sql-appmodassist"
export SqlServerFqdn=""
export DatabaseName="Northwind"

# ─── Entra ID Admin ───────────────────────────────────────────────────────────
export AdminObjectId=""
export AdminLogin=""

# ─── Application Settings ─────────────────────────────────────────────────────
export DotNetFramework="net8.0"
export ConnectionString=""

# ─── Derived / convenience exports ────────────────────────────────────────────
# Make SQL connection details available to Python scripts via env vars
export SQL_SERVER_FQDN="${SqlServerFqdn}"
export SQL_DATABASE="${DatabaseName}"
export AZURE_CLIENT_ID="${ManagedIdentityClientId}"

# ─── Pre-flight check ─────────────────────────────────────────────────────────
# If SqlServerFqdn is empty the Booster-Spec-Agent has not yet populated this
# file. Print a clear error and exit so the caller can act on it.
if [[ -z "${SqlServerFqdn}" ]]; then
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║  ERROR: AgentVariables.sh has not been populated yet.           ║"
    echo "║                                                                  ║"
    echo "║  SqlServerFqdn is empty. Please run the Booster-Spec-Agent     ║"
    echo "║  (deploy-infra.sh) first to provision Azure infrastructure and  ║"
    echo "║  populate this file before running deploy-app.sh.               ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
    exit 1
fi
