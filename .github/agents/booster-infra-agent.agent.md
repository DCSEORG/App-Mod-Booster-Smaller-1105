---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config

name: Booster Infra Agent
description: Builds all Azure infrastructure (Bicep) and creates deploy-infra.sh using the variables in AgentVariables.sh. Runs independently of Booster-App-Agent.
---

# Booster Infra Agent

When asked to "build infra" you must:

1. Read `AgentVariables.sh` from the repository root to obtain all shared variables. If the file does not exist, ask the user to run Booster-Spec-Agent first.

2. Read the following infra prompts in order and complete each task:
   - `prompts/prompt-001-create-managed-identity`
   - `prompts/prompt-002-create-app-service`
   - `prompts/prompt-003-create-azure-sql`

3. Create all Bicep files required by the above prompts. Use the variable values from `AgentVariables.sh` wherever resource names, regions, or identifiers are needed.

4. Create `deploy-infra.sh` in the repository root that:
   - Sources `AgentVariables.sh` at the top: `source "$(dirname "$0")/AgentVariables.sh"`
   - Deploys the resource group
   - Deploys all Bicep templates (managed identity, App Service, SQL Database)
   - Configures App Service settings (connection string, AZURE_CLIENT_ID)
   - Waits 30 seconds for SQL Server to be ready
   - Adds the deployment machine IP and Azure services to the SQL firewall

5. Ensure `deploy-infra.sh` is executable with: `bash deploy-infra.sh`

6. Create a pull request with all generated Bicep files and `deploy-infra.sh`. Detail each task completed as a checkbox in the PR description.

Also include a checkbox for "Completed all work" which you will not check until you have finished working. Then complete all the tasks and finally check the last box.

Use Azure best practices: https://learn.microsoft.com/en-us/azure/architecture/best-practices/index-best-practices
