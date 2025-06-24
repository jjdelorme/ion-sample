using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree;
using Google.Cloud.Storage.V1;
using Google.Cloud.PubSub.V1;
using System.Text;
using System.Text.Json;

var projectId = "your-gcp-project-id";
var bucketName = "your-gcs-bucket-name";
var topicId = "your-pubsub-topic-id";

// Create a sample ION file
var ionStream = new MemoryStream();
using (var writer = IonBinaryWriterBuilder.Build(ionStream))
{
    writer.WriteValues(IonLoader.Default.Load("[{name:\"John Doe\", age:30, city:\"New York\"}, {name:\"Jane Doe\", age:25, city:\"London\"}]"));
    writer.Finish();
}

ionStream.Position = 0;

// Upload to GCS
var storageClient = StorageClient.Create();
var objectName = $"ion-file-{DateTime.UtcNow:yyyyMMddHHmmssfff}.ion";
storageClient.UploadObject(bucketName, objectName, "application/ion", ionStream);

Console.WriteLine($"Uploaded {objectName} to {bucketName}");

// Publish to Pub/Sub
var publisher = await PublisherClient.CreateAsync(new TopicName(projectId, topicId));

var message = new PubsubMessage
{
    Data = Google.Protobuf.ByteString.CopyFrom(
        JsonSerializer.Serialize(new { message = new { data = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { bucket = bucketName, name = objectName }))) } }),
        Encoding.UTF8)
};

var messageId = await publisher.PublishAsync(message);
Console.WriteLine($"Published message {messageId} to {topicId}");