terraform {
  backend "local" {}
  required_version = "~> 1.9.6"
  required_providers {
    azapi = {
      source  = "azure/azapi"
      version = "~> 2.0.1"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.3.0"
    }
    local = {
      source  = "hashicorp/local"
      version = "2.5.2"
    }
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
  subscription_id = var.subscription_id
}
