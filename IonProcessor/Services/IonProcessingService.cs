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
        private readonly IDecompressionService _decompressionService;

        public IonProcessingService(StorageClient storageClient, ILogger<IonProcessingService> logger, IDecompressionService decompressionService)
        {
            _storageClient = storageClient;
            _logger = logger;
            _decompressionService = decompressionService;
        }

        public async Task<string> ProcessIonFileAsync(string bucketName, string objectName)
        {
            _logger.LogInformation($"Processing file: {objectName} from bucket: {bucketName}");

            using (var memoryStream = new MemoryStream())
            {
                await _storageClient.DownloadObjectAsync(bucketName, objectName, memoryStream);
                memoryStream.Position = 0;

                using (var decompressedStream = _decompressionService.Decompress(memoryStream))
                {
                    var ionReader = IonReaderBuilder.Build(decompressedStream);
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
}
