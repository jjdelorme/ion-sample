# Terraform Scripts for IonProcessor

These Terraform scripts will provision the entire Google Cloud infrastructure required to run the `IonProcessor` service.

## Prerequisites

1.  [Terraform](https://learn.hashicorp.com/tutorials/terraform/install-cli) installed.
2.  [Google Cloud SDK](https://cloud.google.com/sdk/docs/install) installed and authenticated (`gcloud auth application-default login`).
3.  A Google Cloud project with billing enabled.
4.  A BigQuery dataset and table must exist in your project.

## What It Deploys

*   **Google Cloud APIs:** Enables all necessary APIs (Cloud Run, Pub/Sub, GCS, Cloud Build, etc.).
*   **Artifact Registry:** A Docker repository to store the `ion-processor` container image.
*   **Google Cloud Storage:** A bucket to store the incoming ION files.
*   **Pub/Sub:** A main topic for processing and a dead-letter topic for failed messages.
*   **Service Accounts:** Dedicated service accounts for the Cloud Run service and Pub/Sub for fine-grained permissions.
*   **IAM Bindings:** The necessary permissions for all the services to communicate securely, including granting the Cloud Build service account permission to write to Artifact Registry.
*   **Cloud Run Service:** The `IonProcessor` service itself.


## How to Use

1.  **Create a `terraform.tfvars` file:**

    Create a file named `terraform.tfvars` in this directory and populate it with the required variables:

    ```hcl
    project_id          = "your-gcp-project-id"
    gcs_bucket_name     = "your-unique-gcs-bucket-name"
    bigquery_dataset_id = "your-bigquery-dataset-id"
    bigquery_table_id   = "your-bigquery-table-id"
    ```

2.  **Initialize Terraform:**

    Open a terminal in this directory and run:

    ```bash
    terraform init
    ```

3.  **Plan the deployment:**

    This command will show you what resources Terraform will create.

    ```bash
    terraform plan
    ```

4.  **Apply the configuration:**

    This command will create all the resources in your Google Cloud project.

    ```bash
    terraform apply
    ```

## Cleaning Up

To destroy all the resources created by these scripts, run:

```bash
terraform destroy
```
