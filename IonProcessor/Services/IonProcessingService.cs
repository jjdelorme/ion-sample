using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.IonDotnet.Builders;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;

namespace IonProcessor.Services
{
    public class IonProcessingService : IIonProcessingService
    {
        private readonly StorageClient _storageClient;
        private readonly ILogger<IonProcessingService> _logger;

        public IonProcessingService(StorageClient storageClient, ILogger<IonProcessingService> logger)
        {
            _storageClient = storageClient;
            _logger = logger;
        }

        public async Task<string> ProcessIonFileAsync(string bucketName, string objectName)
        {
            _logger.LogInformation($"Processing file: {objectName} from bucket: {bucketName}");

            using (var memoryStream = new MemoryStream())
            {
                await _storageClient.DownloadObjectAsync(bucketName, objectName, memoryStream);
                memoryStream.Position = 0;

                var ionReader = IonReaderBuilder.Build(memoryStream);
                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var ionWriter = IonTextWriterBuilder.Build(stringWriter))
                {
                    ionWriter.WriteValues(ionReader);
                }
                return stringBuilder.ToString();
            }
        }
    }
}
