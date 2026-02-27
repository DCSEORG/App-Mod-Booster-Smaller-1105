---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config

name:
description:
---

# My Agent

when asked to 'set up project' (or similar) you must read all the prompts in the prompts folder and decide what variables such as ResourceGroupName will be needed by both the infra and app elements of the project then create a file called ProjectVariables and store these variables such that other agents can read them to use the same variables names in other agentic runs
