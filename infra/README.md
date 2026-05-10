# identity-provider infra

Terraform configuration for the existing `auth.ucedo.io` CloudFront distribution.

## Existing AWS resources

- CloudFront distribution: `E3AZILY3V6LH6H`
- CloudFront domain: `dvodvy00iiin4.cloudfront.net`
- Origin: `p6m6sm9kp6.execute-api.us-east-1.amazonaws.com/prod`
- Custom domain: `auth.ucedo.io`
- ACM certificate: `arn:aws:acm:us-east-1:618148662581:certificate/c0896358-06f5-4e07-93f3-8ec3e8c3d2bd`

The certificate is an ACM `AMAZON_ISSUED` certificate for `auth.ucedo.io`, not a GoDaddy-imported certificate according to ACM. It can be reused by CloudFront.

## First run

Configure Git credentials for `https://github.com/sebasucedo/io.ucedo.git` with a PAT, because the Route53 module is sourced from `io.ucedo`.

```bash
terraform init
terraform workspace select prod || terraform workspace new prod
terraform import aws_cloudfront_distribution.auth E3AZILY3V6LH6H
terraform plan
```

The Route53 alias records for `auth.ucedo.io` are new Terraform-managed resources. I checked hosted zone `Z0302119PV1AI8MTHUTV` and there are currently no `auth.ucedo.io.` records there.
