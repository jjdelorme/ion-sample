# Service Account for the Cloud Run service
resource "google_service_account" "run_sa" {
  account_id   = "${var.service_name}-run-sa"
  display_name = "Service Account for ${var.service_name} Cloud Run service"
}

# Service Account for the Pub/Sub subscription to invoke Cloud Run
resource "google_service_account" "pubsub_sa" {
  account_id   = "${var.service_name}-pubsub-sa"
  display_name = "Service Account for Pub/Sub to invoke Cloud Run"
}



# Grant the Cloud Run service account permission to read from GCS
resource "google_storage_bucket_iam_member" "run_sa_gcs_reader" {
  bucket = google_storage_bucket.bucket.name
  role   = "roles/storage.objectViewer"
  member = "serviceAccount:${google_service_account.run_sa.email}"
}

# Grant the Cloud Run service account permission to write to BigQuery
resource "google_project_iam_member" "run_sa_bigquery_writer" {
  project = var.project_id
  role    = "roles/bigquery.dataEditor"
  member  = "serviceAccount:${google_service_account.run_sa.email}"
}

# Grant the Cloud Run service account permission to read from Artifact Registry
resource "google_project_iam_member" "run_sa_artifact_reader" {
  project = var.project_id
  role    = "roles/artifactregistry.reader"
  member  = "serviceAccount:${google_service_account.run_sa.email}"
}

# Grant the Pub/Sub service account permission to invoke the Cloud Run service
resource "google_cloud_run_v2_service_iam_member" "pubsub_sa_run_invoker" {
  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.service.name
  role     = "roles/run.invoker"
  member   = "serviceAccount:${google_service_account.pubsub_sa.email}"
}



# Grant the Pub/Sub service account permission to write to the dead-letter topic
resource "google_pubsub_topic_iam_member" "pubsub_sa_dead_letter_publisher" {
  project = var.project_id
  topic   = google_pubsub_topic.dead_letter_topic.name
  role    = "roles/pubsub.publisher"
  member  = "serviceAccount:${google_service_account.pubsub_sa.email}"
}

# Grant the Cloud Build service account permission to push to Artifact Registry
resource "google_project_iam_member" "cloudbuild_sa_artifact_writer" {
  project = var.project_id
  role    = "roles/artifactregistry.writer"
  member  = "serviceAccount:${data.google_project.project.number}@cloudbuild.gserviceaccount.com"
}



data "google_project" "project" {}

data "google_storage_project_service_account" "gcs_account" {
  project = var.project_id
}

# Grant the Google-managed Pub/Sub service account permission to publish to the dead-letter topic
resource "google_pubsub_topic_iam_member" "pubsub_service_account_dead_letter_publisher" {
  project = var.project_id
  topic   = google_pubsub_topic.dead_letter_topic.name
  role    = "roles/pubsub.publisher"
  member  = "serviceAccount:service-${data.google_project.project.number}@gcp-sa-pubsub.iam.gserviceaccount.com"
  depends_on = [google_pubsub_topic.dead_letter_topic, data.google_project.project]
}

# Grant the Google-managed Pub/Sub service account permission to consume from the main subscription for dead-lettering
resource "google_pubsub_subscription_iam_member" "pubsub_service_account_dead_letter_subscriber" {
  project      = var.project_id
  subscription = google_pubsub_subscription.subscription.name
  role         = "roles/pubsub.subscriber"
  member       = "serviceAccount:service-${data.google_project.project.number}@gcp-sa-pubsub.iam.gserviceaccount.com"
  depends_on = [google_pubsub_subscription.subscription, data.google_project.project]
}

# Grant the GCS service account permission to publish to the main Pub/Sub topic
resource "google_project_iam_member" "gcs_pubsub_publisher" {
  project = var.project_id
  role    = "roles/pubsub.publisher"
  member  = "serviceAccount:service-${data.google_project.project.number}@gs-project-accounts.iam.gserviceaccount.com"
  depends_on = [
    google_pubsub_topic.main_topic,
    data.google_project.project
  ]
}
