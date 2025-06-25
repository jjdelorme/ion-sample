variable "project_id" {
  description = "The Google Cloud project ID."
  type        = string
}

variable "region" {
  description = "The Google Cloud region to deploy resources into."
  type        = string
  default     = "us-central1"
}

variable "service_name" {
  description = "The name of the Cloud Run service and other related resources."
  type        = string
  default     = "ion-processor"
}

variable "gcs_bucket_name" {
  description = "The name of the GCS bucket for ION files. Must be globally unique."
  type        = string
  default     = "ion-files"
}

variable "bigquery_dataset_id" {
  description = "The ID of the BigQuery dataset."
  type        = string
  default     = "ion_data"
}

variable "bigquery_table_id" {
  description = "The ID of the BigQuery table."
  type        = string
  default     = "processed_ions"
}
