# access the configuration of the azurerm provider.
data "azurerm_client_config" "current" {}

# global naming conventions and resources
module "globals" {
  source = "github.com/brightwavepartners/terraform-azure/modules/globals"

  application = var.application
  environment = var.environment
  location    = var.location
  tenant      = var.tenant
}

# utilitiies, like getting current ip address
module "utilities" {
  source = "github.com/brightwavepartners/terraform-azure/modules/utilities"
}

# find the shared log analytics workspace
data "azurerm_log_analytics_workspace" "log_analytics_workspace" {
  name                = "bwp-shared-dev-ncus-log-law"
  resource_group_name = "bwp-shared-dev-ncus"
}

# resource group
module "resource_group" {
  source = "github.com/brightwavepartners/terraform-azure/modules/resource_group"

  application = var.application
  environment = var.environment
  location    = var.location
  tags        = var.tags
  tenant      = var.tenant
}

# virtual network
module "virtual_network" {
  source = "github.com/brightwavepartners/terraform-azure/modules/virtual_network"

  address_space       = var.virtual_network_address_space
  application         = var.application
  environment         = var.environment
  location            = var.location
  resource_group_name = module.resource_group.name
  tags                = var.tags
  tenant              = var.tenant
}

# app services subnets
#   vnet integration for app services is based on the app service plan that hosts the app service.
#   a subnet delegation (the integration subnet) in a vnet is only allowed to host one app service plan,
#   so a subnet is created for each app service plan that is defined in the configuration.
# app services subnets
module "app_service_subnets" {
  source = "github.com/brightwavepartners/terraform-azure/modules//subnet"

  for_each = {
    for app_service_plan in var.configuration.app_service_plans :
    lower(
      join(
        "-",
        [
          module.globals.resource_base_name_long,
          app_service_plan.role,
          module.globals.object_type_names.subnet
        ]
      )
    ) => app_service_plan
  }

  address_prefixes = [
    cidrsubnet(
      var.virtual_network_address_space[0],
      each.value.subnet.newbits,
      each.value.subnet.netnum
    )
  ]
  application = var.application
  environment = var.environment
  location    = var.location
  name = lower(
    each.value.role
  )
  network_security_group_rules = try(
    each.value.subnet.security_rules,
    []
  )
  resource_group_name                 = module.resource_group.name
  role                                = each.value.role
  tags                                = var.tags
  tenant                              = var.tenant
  virtual_network_name                = module.virtual_network.name
  virtual_network_resource_group_name = module.virtual_network.resource_group_name

  delegation = {
    name = "Microsoft.Web.serverFarms"

    service_delegation = {
      name = "Microsoft.Web/serverFarms"

      actions = [
        "Microsoft.Network/virtualNetworks/subnets/action"
      ]
    }
  }

  service_endpoints = [
    "Microsoft.Storage"
  ]
}

# app service plans
module "app_service_plans" {
  source = "github.com/brightwavepartners/terraform-azure/modules/service_plan"

  for_each = {
    for service_plan in var.configuration.app_service_plans :
    lower(
      join(
        "-",
        [
          module.globals.resource_base_name_long,
          service_plan.role,
          module.globals.object_type_names.app_service_plan
        ]
      )
    ) => service_plan
  }

  application = var.application
  environment = var.environment
  location    = var.location
  maximum_elastic_worker_count = try(
    each.value.maximum_elastic_worker_count,
    1
  )
  os_type             = each.value.os_type
  resource_group_name = module.resource_group.name
  role                = each.value.role
  scale_settings = try(
    each.value.scale_settings,
    []
  )
  sku_name = each.value.sku_name
  tags     = var.tags
  tenant   = var.tenant
}

# function apps - signalr hub
module "functions" {
  source = "github.com/brightwavepartners/terraform-azure/modules/function_app"

  for_each = {
    for function_app in var.configuration.function_apps :
    lower(
      join(
        "-",
        [
          module.globals.resource_base_name_long,
          function_app.role,
          module.globals.object_type_names.function_app
        ]
      )
    ) => function_app
  }

  app_settings = try(
    each.value.app_settings,
    {}
  )
  application       = var.application
  application_stack = each.value.application_stack
  cors_settings     = each.value.cors_settings
  environment       = var.environment
  ip_restrictions = try(
    each.value.ip_restrictions,
    []
  )
  location                   = var.location
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.log_analytics_workspace.id
  resource_group_name        = module.resource_group.name
  role                       = each.value.role
  service_plan_id = element(
    [
      for app_service_plan in module.app_service_plans : app_service_plan.id
      if can(
        regex(
          lower(
            each.value.service_plan_role
          ),
          app_service_plan.name
        )
      )
    ],
    0
  )
  storage = {
    alert_settings = []
    vnet_integration = {
      allowed_ips = [
        module.utilities.ip
      ]
      enabled                   = true
      file_share_retention_days = 0
    }
  }
  tags              = var.tags
  tenant            = var.tenant
  type              = each.value.type
  use_32_bit_worker = false
  vnet_integration = {
    subnet_id = element(
      [
        for app_service_subnet in module.app_service_subnets : app_service_subnet.id
        if can(
          regex(
            lower(
              each.value.service_plan_role
            ),
            app_service_subnet.name
          )
        )
      ],
      0
    )
    vnet_route_all_enabled = true
  }
}
