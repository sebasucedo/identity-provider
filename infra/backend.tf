terraform {
  backend "s3" {
    bucket               = "ucedo-terraform-state"
    key                  = "terraform.tfstate"
    region               = "us-east-1"
    encrypt              = true
    workspace_key_prefix = "identity-provider/env"
  }
}

