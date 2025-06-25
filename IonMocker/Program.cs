using Amazon.IonDotnet.Builders;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var bucketName = configuration["Gcp:BucketName"];

// Create a sample ION file
var ionStream = new MemoryStream();
using (var writer = IonBinaryWriterBuilder.Build(ionStream))
{
    var json = "[{name:\"John Doe\", age:30, city:\"New York\"}, {name:\"Jane Doe\", age:25, city:\"London\"}]";
    var reader = IonReaderBuilder.Build(json);
    writer.WriteValues(reader);
    writer.Finish();
}

ionStream.Position = 0;

// Upload to GCS
var storageClient = StorageClient.Create();
var objectName = $"ion-file-{DateTime.UtcNow:yyyyMMddHHmmssfff}.ion";
storageClient.UploadObject(bucketName, objectName, "application/ion", ionStream);

Console.WriteLine($"Uploaded {objectName} to {bucketName}");
