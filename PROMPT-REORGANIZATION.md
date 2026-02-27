# Prompt Reorganization Summary

## Overview
Reorganized 25+ prompts into 10 focused, actionable prompts to optimize agent performance, and introduced a three-agent workflow for parallel infrastructure and application deployment.

## Key Changes

### Before
- 25+ prompt files with overlapping concerns
- Redundant instructions scattered across files
- Required significant planning from the agent
- Mixed multiple functionalities in single prompts
- Verbose explanations requiring interpretation
- Single monolithic agent

### After
- 10 streamlined prompts, one per discrete functionality
- All considerations retained and consolidated
- Minimal planning required - each prompt is immediately actionable
- Clear separation of concerns between infra and app prompts
- Three specialized agents enabling parallel deployments

## New Prompt Structure

1. **prompt-001-create-managed-identity** - Creates user-assigned managed identity
2. **prompt-002-create-app-service** - Creates App Service with identity
3. **prompt-003-create-azure-sql** - Creates Azure SQL with Entra ID auth
4. **prompt-004-import-database-schema** - Python script to import schema
5. **prompt-005-configure-database-roles** - Sets up managed identity DB permissions
6. **prompt-006-create-stored-procedures** - Creates stored procs for data access
7. **prompt-007-create-application-code** - Creates ASP.NET Razor Pages app
8. **prompt-008-create-api-endpoints** - Creates REST APIs with Swagger
9. **prompt-009-create-deployment-scripts** - Creates deploy-infra.sh and deploy-app.sh
10. **prompt-013-create-architecture-diagram** - Creates architecture diagram

## Three-Agent Workflow

### Booster-Spec-Agent
Reads all prompts and produces `AgentVariables.sh` containing every shared variable needed by both infra and app agents. Run this first so the other two agents can operate in parallel.

### Booster-Infra-Agent
Uses infra prompts (001-003) to build all Azure Bicep templates and creates `deploy-infra.sh`. Sources `AgentVariables.sh` for all shared variable values.

### Booster-App-Agent
Uses app prompts (004-008) to build all application code and database scripts and creates `deploy-app.sh`. Sources `AgentVariables.sh` for all shared variable values.

Booster-Infra-Agent and Booster-App-Agent can run **in parallel** after Booster-Spec-Agent completes because they share no outputs with each other - both draw their variables from `AgentVariables.sh`.

## Consolidated Considerations

All critical considerations from original prompts are retained:

### Security & Authentication
- Entra ID (Azure AD) only authentication
- Managed identity for all Azure service connections
- No SQL authentication, no API keys
- MCAPS governance policy compliance

### Technical Requirements
- .NET 8 (LTS) target framework
- Lowercase resource naming
- uniqueString() for resource uniqueness (no timestamps)
- Stable API versions (@2021-11-01, not preview)
- Cross-platform script compatibility (Mac/Linux)

### Deployment Best Practices
- 30-second waits for resource readiness
- Proper deployment order
- SQL firewall configuration (current IP + Azure services)
- Shared AgentVariables.sh sourced by both deploy scripts
- App.zip at root level (not nested)

### Database Access Patterns
- Stored procedures only (no direct SQL in app)
- No direct table access
- APIs as single source of truth
- Managed identity authentication

### Error Handling
- Dummy data fallback on connection failures
- Detailed error messages with actionable fixes
- User-friendly error displays

## Performance Improvements

1. **Reduced context switching** - Agent processes one clear task at a time
2. **Eliminated redundancy** - No duplicate instructions across prompts
3. **Minimal planning** - Each prompt is self-contained and actionable
4. **Clear dependencies** - Sequential order matches infrastructure dependencies
5. **Focused scope** - Each prompt has single responsibility
6. **Parallel deployment** - Infra and app agents can run simultaneously

## Execution Flow

```
Booster-Spec-Agent  →  generates AgentVariables.sh
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
  Booster-Infra-Agent               Booster-App-Agent
  (prompts 001-003)                 (prompts 004-008)
  → Bicep files                     → App code
  → deploy-infra.sh                 → deploy-app.sh
```

Both deploy scripts source `AgentVariables.sh` so resource names (e.g. resource group) are consistent across infra and app deployments.

## Result

The reorganized prompts and three-agent model enable:
- Faster execution through parallelism
- Clear separation of infrastructure and application concerns
- Independent deployability of infra and app layers
- Consistent shared variables across all deployment scripts
- No GenAI dependencies in the core workflow
