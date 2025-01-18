# TODO:
# create app registration for signalr service

locals {
  primary_region_resource_group = element(
    [
      for region in module.regions : region.resource_group
      if can(
        regex(
          region.resource_group.location,
          var.configuration.regions.primary_region.location
        )
      )
    ],
    0
  )
}

# global naming conventions and resources
module "globals" {
  source = "github.com/brightwavepartners/terraform-azure/modules/globals"

  application = var.application
  environment = var.environment
  location    = var.configuration.regions.primary_region.location
  tenant      = var.tenant
}

#####################################################################
# create regional resources that are duplicated across every region #
#####################################################################

# regions
module "regions" {
  source = "./region"

  for_each = {
    for region in flatten(
      [
        var.configuration.regions.primary_region,
        var.configuration.regions.auxiliary_regions
      ]
    ) :
    join(
      "-",
      [
        var.tenant,
        var.application,
        var.environment,
        region.location
      ]
    ) => region
  }

  application                   = var.application
  configuration                 = var.configuration
  environment                   = var.environment
  location                      = each.value.location
  tags                          = var.tags
  tenant                        = var.tenant
  virtual_network_address_space = each.value.virtual_network.address_space
}

################################################################################################
# create and/or configure any additional resources that are not duplicated across every region #
# but are dependant on regional resources having been created first                            #
################################################################################################

# serverless signalr
module "signalr" {
  source = "github.com/brightwavepartners/terraform-azure/modules/signal_r"

  application               = var.application
  capacity                  = var.configuration.signalr.capacity
  connectivity_logs_enabled = var.configuration.signalr.connectivity_logs_enabled
  cors                      = var.configuration.signalr.cors
  environment               = var.environment
  location                  = var.configuration.regions.primary_region.location
  messaging_logs_enabled    = var.configuration.signalr.messaging_logs_enabled
  replicas                  = var.configuration.signalr.replicas
  resource_group_name       = local.primary_region_resource_group.name
  service_mode              = var.configuration.signalr.service_mode
  sku                       = var.configuration.signalr.tier
  tags                      = var.tags
  tenant                    = var.tenant
}

