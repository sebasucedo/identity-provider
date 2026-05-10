locals {
  project_name = "identity-provider"
  environment  = terraform.workspace
  zone_name    = "ucedo.io"

  domain_by_workspace = {
    prod = "auth.ucedo.io"
  }

  domain_name  = var.domain_name != "" ? var.domain_name : lookup(local.domain_by_workspace, local.environment, "")
  domain_names = local.domain_name != "" ? [local.domain_name] : []

  should_lookup_route53_zone = length(local.domain_names) > 0 && var.route53_hosted_zone_id == null

  common_tags = merge(
    {
      Environment = local.environment
      Project     = local.project_name
      ManagedBy   = "terraform"
    },
    var.tags
  )
}

