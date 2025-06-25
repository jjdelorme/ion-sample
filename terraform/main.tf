terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = ">= 4.50.0"
    }
  }
}

provider "google" {
  project = var.project_id
  region  = var.region
}

# Enable necessary APIs
resource "google_project_service" "apis" {
  for_each = toset([
    "run.googleapis.com",
    "pubsub.googleapis.com",
    "storage.googleapis.com",
    "artifactregistry.googleapis.com",
    "iam.googleapis.com",
    "bigquery.googleapis.com",
    "cloudbuild.googleapis.com"
  ])

  service            = each.key
  disable_on_destroy = false
}

# Artifact Registry for Docker images
resource "google_artifact_registry_repository" "repo" {
  provider      = google
  location      = var.region
  repository_id = "${var.service_name}-repo"
  description   = "Docker repository for ${var.service_name}"
  format        = "DOCKER"
  depends_on    = [google_project_service.apis]
}

# GCS Bucket for ION files
resource "google_storage_bucket" "bucket" {
  name          = var.gcs_bucket_name
  location      = var.region
  force_destroy = true # Set to false in production if you want to prevent accidental deletion
  uniform_bucket_level_access = true
  depends_on    = [google_project_service.apis]
}

# GCS Storage Notification
resource "google_storage_notification" "notification" {
  bucket         = google_storage_bucket.bucket.name
  payload_format = "JSON_API_V1"
  topic          = google_pubsub_topic.main_topic.id
  event_types    = ["OBJECT_FINALIZE"]
  depends_on = [
    google_storage_bucket.bucket,
    google_pubsub_topic.main_topic
  ]
}

# BigQuery Dataset
resource "google_bigquery_dataset" "dataset" {
  dataset_id = var.bigquery_dataset_id
  location   = var.region
  depends_on = [google_project_service.apis]
}

# BigQuery Table
resource "google_bigquery_table" "table" {
  dataset_id = google_bigquery_dataset.dataset.dataset_id
  table_id   = var.bigquery_table_id
  schema = jsonencode([
    {
      "name" : "data",
      "type" : "STRING",
      "mode" : "NULLABLE",
      "description" : "The raw ION data as a string."
    }
  ])
  depends_on = [google_bigquery_dataset.dataset]
}



# Pub/Sub Topics
resource "google_pubsub_topic" "main_topic" {
  name = "${var.service_name}-topic"
  depends_on = [google_project_service.apis]
}

resource "google_pubsub_topic" "dead_letter_topic" {
  name = "${var.service_name}-dead-letter-topic"
  depends_on = [google_project_service.apis]
}

# Cloud Run Service
resource "google_cloud_run_v2_service" "service" {
  name     = var.service_name
  location = var.region
  deletion_protection = false

  template {
    service_account = google_service_account.run_sa.email

    containers {
      image = "us-docker.pkg.dev/cloudrun/container/hello"
      ports {
        container_port = 8080
      }
      env {
        name  = "GoogleCloud__ProjectId"
        value = var.project_id
      }
      env {
        name  = "GoogleCloud__DatasetId"
        value = var.bigquery_dataset_id
      }
      env {
        name  = "GoogleCloud__TableId"
        value = var.bigquery_table_id
      }
    }
  }
  depends_on = [google_project_service.apis]
}

# Pub/Sub Push Subscription
resource "google_pubsub_subscription" "subscription" {
  name  = "${var.service_name}-subscription"
  topic = google_pubsub_topic.main_topic.name

  push_config {
    push_endpoint = "${google_cloud_run_v2_service.service.uri}/PubSub"
    oidc_token {
      service_account_email = google_service_account.pubsub_sa.email
    }
  }

  dead_letter_policy {
    dead_letter_topic = google_pubsub_topic.dead_letter_topic.id
    max_delivery_attempts = 5
  }

  depends_on = [
    google_cloud_run_v2_service.service,
    google_service_account.pubsub_sa
  ]
}

# Subscription for the dead-letter topic
resource "google_pubsub_subscription" "dead_letter_subscription" {
  name  = "${var.service_name}-dead-letter-subscription"
  topic = google_pubsub_topic.dead_letter_topic.name
  ack_deadline_seconds = 600 # Or another appropriate value
  message_retention_duration = "604800s" # 7 days
  depends_on = [google_pubsub_topic.dead_letter_topic]
}


