# Gemini Project Context: Ion Processor

This file helps me remember the work I've done on the `ion` project for you.

## Project Goal

The user wants to create a .NET Web API that can be hosted on Google Cloud Run. This API will process binary ION files that are stored in a Google Cloud Storage bucket. The processing is triggered by a Google Cloud Pub/Sub push notification that contains a reference to the ION file. The API reads the ION file, and writes its contents to a Google BigQuery table.

The user emphasized the need for a scalable and reliable solution, with proper handling of message acknowledgements (ack/nack) in case of processing success or failure.

## My Implementation Steps

1.  **Scaffolded a new .NET Web API project** in the `IonProcessor` directory, initially targeting .NET 9 and later migrating to .NET 8.
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
10. **Created a new xUnit test project** (`IonProcessor.Tests`) and added comprehensive unit tests for the controller and both services, ensuring the application is well-tested and reliable.
11. **Created a `README.md`** with project overview and deployment instructions.
12. **Created this `GEMINI.md`** file to maintain context.
13. **Created the `IonMocker` console application** to generate test ION files, upload them to GCS, and publish notifications to Pub/Sub for end-to-end testing.

## Development Rules

* Don’t try to be clever
* Build the simplest code possible that will pass the test
* With each change practice self reflection:
    - How difficult was it to write?
    - How hard is it to understand?
    - How expensive will it be to change?
* Code should be concrete enough to be understood while simultaneously being abstract enough to	allow for change.
* Code is easy to understand when it clearly reflects the problem it’s solving, and thus openly	exposes that problem’s domain.
* Isolate things that change from things that don’t change (high cohesion, loose coupling)
* Each method should have a Single Responsibility and methods should be consistent. Consistent code enables future refactorings.
* Follow the SOLID principles 
* Seek stable landing points and create a git commit when the application complies and tests pass before moving on to the next functional change.

### Project creation
* Remove any unused boiler plate code generated from new project creation by the frameworks (i.e. Weather service in .NET).

### Test Driven Development

The application uses xUnit for Unit Tests, all development is done using red-green-refactor test driven development. 

* Write tests that confirm what your code does first without any knowledge of how your code does it.
* Tests are not the place for abstractions—they are the place for concretions. Abstractions belong in code.
* When faced with a new requirement, first rearrange the existing code such that it's open to the new feature, and once that's complete, then add the new code
* When refactoring, follow the flocking rules: 
    1. Select the things that are most alike.
    2. Find the smallest difference between them.
    3. Make the simplest change to remove that difference

### ASP.NET Rules
* Ensure that controller routes are lower case.
* Ensure that CORS support is available in development mode for all hosts, all headers
* Ensure that the Swagger UI is available in development 
