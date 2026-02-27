#!/bin/bash
# deploy-app.sh
# Deploys database objects and application code.
# Must be run AFTER deploy-infra.sh has populated AgentVariables.sh.
#
# Usage:
#   bash deploy-app.sh
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ── Load shared variables ────────────────────────────────────────────────────
# shellcheck source=AgentVariables.sh
source "${SCRIPT_DIR}/AgentVariables.sh"

echo ""
echo "╔══════════════════════════════════════════════════════════════════╗"
echo "║  Expense App – Deployment                                        ║"
echo "╚══════════════════════════════════════════════════════════════════╝"
echo ""
echo "  Resource Group : ${ResourceGroupName}"
echo "  App Service    : ${AppServiceName}"
echo "  SQL Server     : ${SqlServerFqdn}"
echo "  Database       : ${DatabaseName}"
echo ""

cd "${SCRIPT_DIR}"

# ── 1. Install Python dependencies ──────────────────────────────────────────
echo "── Step 1: Installing Python dependencies ──"
pip3 install --quiet pyodbc azure-identity
echo "✓  Python dependencies installed"
echo ""

# ── 2. Import database schema ────────────────────────────────────────────────
echo "── Step 2: Importing database schema ──"
python3 run-sql.py
echo ""

# ── 3. Configure database roles ──────────────────────────────────────────────
echo "── Step 3: Configuring database roles for managed identity ──"
python3 run-sql-dbrole.py
echo ""

# ── 4. Create stored procedures ──────────────────────────────────────────────
echo "── Step 4: Deploying stored procedures ──"
python3 run-sql-stored-procs.py
echo ""

# ── 5. Build and package the application ─────────────────────────────────────
echo "── Step 5: Building application ──"
cd "${SCRIPT_DIR}/src/ExpenseApp"
dotnet publish -c Release -o publish
echo "✓  Application built"
echo ""

echo "── Step 5b: Creating deployment package ──"
cd "${SCRIPT_DIR}/src/ExpenseApp/publish"
# Use -r to recursively include all files and subdirectories (wwwroot/css, etc.)
zip -r "${SCRIPT_DIR}/src/ExpenseApp/app.zip" .
echo "✓  app.zip created"
echo ""

# ── 6. Deploy to App Service ─────────────────────────────────────────────────
echo "── Step 6: Deploying to Azure App Service ──"
cd "${SCRIPT_DIR}"
az webapp deploy \
    --resource-group "${ResourceGroupName}" \
    --name            "${AppServiceName}" \
    --src-path        "${SCRIPT_DIR}/src/ExpenseApp/app.zip" \
    --type            zip
echo ""
echo "✓  Deployment complete!"
echo ""
echo "  App URL : https://${AppServiceName}.azurewebsites.net/Index"
echo "  API Docs: https://${AppServiceName}.azurewebsites.net/swagger"
echo ""
