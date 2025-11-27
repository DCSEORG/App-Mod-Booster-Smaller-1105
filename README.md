<img width="473" height="38" alt="image" src="https://github.com/user-attachments/assets/700b0b96-d5a1-4b4b-85fe-c59d16edd21a" />![Header image](https://github.com/DougChisholm/App-Mod-Booster/blob/main/repo-header-booster.png)

# App-Mod-Booster
A project to show how GitHub coding agent can turn screenshots of a legacy app into a working proof-of-concept for a cloud native Azure replacement if the legacy database schema is also provided.

Steps to modernise an app:

1. Fork this repo 
2. In new repo replace the screenshots and sql schema (or keep the samples)
3. Open the coding agent and use app-mod-booster agent telling it "modernise my app"
4. When the app code is generated (can take up to 30 minutes) there will be a pull request to approve.
5. Now you can open VS Code and clone the repo 
6. Open terminal in VS Code and using the Azure CLI run "az login" to set subscription/context
7. Run the deploy.sh file (ensuring the settings in the bicep files are what you want - it will have RG name, SKU, UKSOUTH etc already set)

Note the script current requires python to be installed.

Example video:

Supporting slides for Microsoft Employees:
https://microsofteur-my.sharepoint.com/:p:/g/personal/dchisholm_microsoft_com/IQAY41LQ12fjSIfFz3ha4hfFAZc7JQQuWaOrF7ObgxRK6f4?e=p6arJs
