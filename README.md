# Overview

This is a monorepo for a sample SignalR implementation built on [Azure Serverless SignalR](https://learn.microsoft.com/en-us/azure/azure-signalr/signalr-concept-azure-functions).

There are four components to the implementation.

#### Client

The Client folder contains a minimal React application used to demonstrate and test the interaction with the SignalR service.

#### IaC

The IaC folder contains the Terraform code that is required to provision necessary Azure resources.

#### Service

The Service folder contains the Azure Function application that servers as the SignalR service.

#### Api (Coming Soon)

The Api folder contains a minimal C# application that serves as an Api application that connects the SignalR service and broadcasts messages to SignalR clients.

## Development Setup

#### Visual Studio Dev Containers

The IaC and Client code is built to be used inside of [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/devcontainers/containers). You don't have to use the dev containers, but the containers contain all the necessary tooling at the specific versions required for each project (e.g., Terraform for IaC, React for Client). Using dev containers negates the need to install all the tooling locally and manage versions.
