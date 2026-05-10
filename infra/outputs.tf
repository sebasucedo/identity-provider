output "environment" {
  description = "Current Terraform workspace."
  value       = local.environment
}

output "cloudfront_distribution_id" {
  description = "CloudFront distribution ID managed by this configuration."
  value       = aws_cloudfront_distribution.auth.id
}

output "cloudfront_distribution_arn" {
  description = "CloudFront distribution ARN."
  value       = aws_cloudfront_distribution.auth.arn
}

output "cloudfront_domain_name" {
  description = "CloudFront distribution domain name."
  value       = aws_cloudfront_distribution.auth.domain_name
}

output "custom_domain_url" {
  description = "Custom domain URL."
  value       = local.domain_name != "" ? "https://${local.domain_name}" : null
}

output "certificate_arn" {
  description = "ACM certificate ARN used by CloudFront."
  value       = var.acm_certificate_arn
}

output "route53_zone_id" {
  description = "Route53 hosted zone ID used for alias records."
  value       = length(module.route53_alias) > 0 ? module.route53_alias[0].zone_id : null
}

