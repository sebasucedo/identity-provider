terraform {
  required_version = ">= 1.3.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region  = var.region
  profile = var.aws_profile

  default_tags {
    tags = local.common_tags
  }
}

resource "aws_cloudfront_distribution" "auth" {
  enabled         = true
  is_ipv6_enabled = true
  price_class     = var.price_class
  aliases         = local.domain_names
  comment         = ""
  http_version    = "http2"

  origin {
    domain_name = var.origin_domain_name
    origin_id   = var.origin_id
    origin_path = "/prod"

    connection_attempts = 3
    connection_timeout  = 10

    custom_origin_config {
      http_port                = 80
      https_port               = 443
      origin_protocol_policy   = "https-only"
      origin_ssl_protocols     = ["TLSv1.2"]
      origin_read_timeout      = 30
      origin_keepalive_timeout = 5
    }
  }

  default_cache_behavior {
    target_origin_id           = var.origin_id
    viewer_protocol_policy     = "allow-all"
    allowed_methods            = ["GET", "HEAD"]
    cached_methods             = ["GET", "HEAD"]
    cache_policy_id            = "4135ea2d-6df8-44a3-9df3-4b5a84be39ad"
    origin_request_policy_id   = "b689b0a8-53d0-40ab-baf2-68738e2966ac"
    response_headers_policy_id = "60669652-455b-4ae9-85a4-c4c02393f86c"
    compress                   = true
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn            = var.acm_certificate_arn
    cloudfront_default_certificate = false
    ssl_support_method             = "sni-only"
    minimum_protocol_version       = "TLSv1.2_2021"
  }

  tags = local.common_tags
}

module "route53_alias" {
  source = "git::https://github.com/sebasucedo/io.ucedo.git//infra/modules/route53?ref=main"

  count = length(local.domain_names) > 0 ? 1 : 0

  zone_name            = local.zone_name
  hosted_zone_id       = var.route53_hosted_zone_id
  lookup_hosted_zone   = local.should_lookup_route53_zone
  domain_names         = local.domain_names
  alias_domain_name    = aws_cloudfront_distribution.auth.domain_name
  alias_zone_id        = aws_cloudfront_distribution.auth.hosted_zone_id
  create_alias_records = true
  create_ipv6_records  = true
  create_certificate   = false
  create_hosted_zone   = false

  tags = local.common_tags
}
