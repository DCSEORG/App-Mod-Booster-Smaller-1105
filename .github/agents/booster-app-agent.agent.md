---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config

name: Booster App Agent
description: Builds the application code, database scripts and creates deploy-app.sh using the variables in AgentVariables.sh. Runs independently of Booster-Infra-Agent.
---

# Booster App Agent

When asked to "build app" you must:

1. Read `AgentVariables.sh` from the repository root to obtain all shared variables. If the file does not exist, ask the user to run Booster-Spec-Agent first.

2. Read the following app prompts in order and complete each task:
   - `prompts/prompt-004-import-database-schema`
   - `prompts/prompt-005-configure-database-roles`
   - `prompts/prompt-006-create-stored-procedures`
   - `prompts/prompt-007-create-application-code`
   - `prompts/prompt-008-create-api-endpoints`

3. Also read `prompts/prompt-013-create-architecture-diagram` and create the architecture diagram.

4. Use the variable values from `AgentVariables.sh` wherever SQL server FQDNs, database names, managed identity names, or other shared configuration are needed in the generated code.

5. Create `deploy-app.sh` in the repository root that:
   - Sources `AgentVariables.sh` at the top: `source "$(dirname "$0")/AgentVariables.sh"`
   - Installs Python dependencies: `pip3 install --quiet pyodbc azure-identity`
   - Imports the database schema: `python3 run-sql.py`
   - Configures database roles: `python3 run-sql-dbrole.py`
   - Creates stored procedures: `python3 run-sql-stored-procs.py`
   - Builds and packages the application: `dotnet publish` then zips output with files at ZIP root
   - Deploys `app.zip` to the App Service

6. Ensure `deploy-app.sh` is executable with: `bash deploy-app.sh`

7. Create a pull request with all generated application code, Python scripts, SQL scripts and `deploy-app.sh`. Detail each task completed as a checkbox in the PR description.

Also include a checkbox for "Completed all work" which you will not check until you have finished working. Then complete all the tasks and finally check the last box.

Use Azure best practices: https://learn.microsoft.com/en-us/azure/architecture/best-practices/index-best-practices
