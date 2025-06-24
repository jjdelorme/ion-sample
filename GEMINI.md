# Gemini Project Context: Ion Processor

This file helps me remember the work I've done on the `ion` project for you.

## Project Goal

The user wants to create a .NET Web API that can be hosted on Google Cloud Run. This API will process binary ION files that are stored in a Google Cloud Storage bucket. The processing is triggered by a Google Cloud Pub/Sub push notification that contains a reference to the ION file. The API reads the ION file, and writes its contents to a Google BigQuery table.

The user emphasized the need for a scalable and reliable solution, with proper handling of message acknowledgements (ack/nack) in case of processing success or failure.

## My Implementation Steps

1.  **Scaffolded a new .NET Web API project** in the `IonProcessor` directory.
2.  **Added NuGet packages:**
    *   `Amazon.IonDotnet`
    *   `Google.Cloud.PubSub.V1`
    *   `Google.Cloud.Storage.V1`
    *   `Google.Cloud.BigQuery.V2`
3.  **Created a `PubSubController`** with a single `POST` endpoint at `/PubSub` to receive the push notifications.
4.  **Implemented the core logic** in the controller to:
    *   Deserialize the incoming Pub/Sub message.
    *   Extract the GCS bucket and object name.
    *   Download the ION file from GCS.
    *   Use `Amazon.IonDotnet` to read the binary ION data.
    *   Insert the data into a BigQuery table.
5.  **Configured Dependency Injection** for `StorageClient` and `BigQueryClient` to improve performance and resource management.
6.  **Externalized configuration** for GCP project ID, dataset ID, and table ID into `appsettings.json`.
7.  **Created a `Dockerfile`** to containerize the application for deployment to Google Cloud Run.
8.  **Cleaned up the default `Program.cs`** to remove the weather forecast example and correctly register the controllers.
9.  **Refactored the core logic** out of the `PubSubController` and into `IonProcessingService` and `BigQueryService` to improve testability and separation of concerns.
10. **Added a placeholder decompression service** (`IDecompressionService` and `NoOpDecompressionService`) to the `IonProcessingService` to prepare for handling compressed ION files.
11. **Created a new xUnit test project** (`IonProcessor.Tests`) and added comprehensive unit tests for the controller and both services, ensuring the application is well-tested and reliable.
12. **Created a `README.md`** with project overview and deployment instructions.
13. **Created this `GEMINI.md`** file to maintain context.

## My Development Philosophy Reminders

*   **Test-Driven Development (TDD):** When adding new features, I should write the tests first to define the desired behavior and then implement the code to make the tests pass.
*   **Refactor Under Green:** I will only refactor the code when all existing tests are passing. This ensures that I don't introduce regressions while improving the code's design.
*   **Comprehensive Testing:** I will strive to write tests that cover all critical paths, including success cases, error conditions, and edge cases.
