variable "region" {
  description = "AWS region for regional resources and provider configuration."
  type        = string
  default     = "us-east-1"
}

variable "aws_profile" {
  description = "AWS profile to use locally. Leave null to use the default credential chain."
  type        = string
  default     = null
}

variable "domain_name" {
  description = "Custom domain name for CloudFront. Defaults to auth.ucedo.io in the prod workspace."
  type        = string
  default     = ""
}

variable "route53_hosted_zone_id" {
  description = "Existing Route53 hosted zone ID for ucedo.io. Leave null to look it up by name."
  type        = string
  default     = null
}

variable "origin_domain_name" {
  description = "API Gateway domain currently used as the CloudFront origin."
  type        = string
  default     = "p6m6sm9kp6.execute-api.us-east-1.amazonaws.com"
}

variable "origin_id" {
  description = "CloudFront origin ID."
  type        = string
  default     = "p6m6sm9kp6.execute-api.us-east-1.amazonaws.com"
}

variable "acm_certificate_arn" {
  description = "Existing ACM certificate ARN in us-east-1 for auth.ucedo.io."
  type        = string
  default     = "arn:aws:acm:us-east-1:618148662581:certificate/c0896358-06f5-4e07-93f3-8ec3e8c3d2bd"
}

variable "cloudfront_distribution_id" {
  description = "Existing CloudFront distribution ID. Used for import documentation and outputs."
  type        = string
  default     = "E3AZILY3V6LH6H"
}

variable "price_class" {
  description = "CloudFront price class."
  type        = string
  default     = "PriceClass_All"
}

variable "tags" {
  description = "Additional tags to apply to supported resources."
  type        = map(string)
  default     = {}
}
