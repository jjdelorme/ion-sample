output "cloud_run_service_url" {
  description = "The URL of the deployed Cloud Run service."
  value       = google_cloud_run_v2_service.service.uri
}

output "gcs_bucket_name" {
  description = "The name of the GCS bucket for ION files."
  value       = google_storage_bucket.bucket.name
}

output "main_pubsub_topic" {
  description = "The name of the main Pub/Sub topic."
  value       = google_pubsub_topic.main_topic.name
}

output "dead_letter_pubsub_topic" {
  description = "The name of the dead-letter Pub/Sub topic."
  value       = google_pubsub_topic.dead_letter_topic.name
}

output "artifact_registry_repository" {
  description = "The name of the Artifact Registry repository."
  value       = google_artifact_registry_repository.repo.name
}
