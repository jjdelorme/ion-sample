using Amazon.IonDotnet.Builders;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var bucketName = configuration["Gcp:BucketName"];
var uploadInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Throttling:UploadIntervalSeconds", 5));
var chunkSize = configuration.GetValue<int>("Throttling:ChunkSizeBytes", ResumableUpload.MinimumChunkSize);

while (true)
{
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

    // Upload to GCS using resumable, chunked uploads
    var storageClient = StorageClient.Create();
    var objectName = $"ion-file-{DateTime.UtcNow:yyyyMMddHHmmssfff}.ion";
    
    var options = new UploadObjectOptions
    {
        ChunkSize = chunkSize
    };

    storageClient.UploadObject(bucketName, objectName, "application/ion", ionStream, options);

    Console.WriteLine($"Uploaded {objectName} to {bucketName}");

    Thread.Sleep(uploadInterval);
}

