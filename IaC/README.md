### Environment Variables
- TF_VAR_subscription_id
- TF_VAR_tenant

If running in a container, these can be found in the .devcontainer/devcontainer.env file

### Execution

terraform apply -var-file="environments/dev.tfvars"