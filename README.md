![Header image](https://github.com/DougChisholm/App-Mod-Booster/blob/main/repo-header-booster.png)

# App-Mod-Booster
A project to show how GitHub coding agents can turn screenshots of a legacy app into a working proof-of-concept for a cloud native Azure replacement if the legacy database schema is also provided.

## Three-Agent Workflow

The modernisation process uses three specialised agents that can run in parallel after an initial setup step:

| Agent | Trigger | What it does |
|---|---|---|
| **Booster-Spec-Agent** | "generate agent variables" | Reads all prompts and creates `AgentVariables.sh` with every shared variable |
| **Booster-Infra-Agent** | "build infra" | Creates Bicep templates + `deploy-infra.sh` (uses `AgentVariables.sh`) |
| **Booster-App-Agent** | "build app" | Creates app code + `deploy-app.sh` (uses `AgentVariables.sh`) |

Booster-Infra-Agent and Booster-App-Agent can run **simultaneously** because they draw all shared values from `AgentVariables.sh` and have no dependencies on each other's outputs.

## Steps to Modernise an App

1. Fork this repo
2. In the new repo replace the screenshots and SQL schema (or keep the samples)
3. Open the coding agent and use **Booster-Spec-Agent** telling it "generate agent variables"
4. Review and edit the generated `AgentVariables.sh` with your target Azure values
5. Trigger **Booster-Infra-Agent** ("build infra") and **Booster-App-Agent** ("build app") — these can run at the same time
6. Approve the pull requests when ready
7. Open a Codespace (or VS Code with the devcontainer) and run `az login` to set your subscription/context
8. Deploy infrastructure: `bash deploy-infra.sh`
9. Deploy application: `bash deploy-app.sh`

> **Note:** App URL is `https://<your-app-name>.azurewebsites.net/Index` (not the root path)

Supporting slides for Microsoft Employees:
[Here](<https://microsofteur-my.sharepoint.com/:p:/g/personal/dchisholm_microsoft_com/IQAY41LQ12fjSIfFz3ha4hfFAZc7JQQuWaOrF7ObgxRK6f4?e=p6arJs>)
