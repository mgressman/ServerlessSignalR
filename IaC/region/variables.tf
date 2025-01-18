variable "application" {
  type        = string
  description = "The name of the application the infrastructure is being provisioned for."
}

variable "configuration" {
  type        = any
  description = "A configuration data file that will be used to configure the new region's resources."
}

variable "environment" {}

variable "location" {
  type        = string
  description = "The region in which to provision the resources."
}

variable "tags" {}

variable "tenant" {
  type        = string
  description = "The name of the organization that the infrastructure is being provisioned for."
}

variable "virtual_network_address_space" {
  type        = list(string)
  description = "The address space(s) to provision for the region's virtual network."
}

