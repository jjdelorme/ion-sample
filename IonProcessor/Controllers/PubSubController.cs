using System;
using Amazon.IonDotnet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.IonDotnet.Builders;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace IonProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PubSubController : ControllerBase
    {
        private readonly ILogger<PubSubController> _logger;
        private readonly StorageClient _storageClient;
        private readonly BigQueryClient _bigQueryClient;
        private readonly GoogleCloudOptions _options;

        public PubSubController(ILogger<PubSubController> logger, StorageClient storageClient, BigQueryClient bigQueryClient, IOptions<GoogleCloudOptions> options)
        {
            _logger = logger;
            _storageClient = storageClient;
            _bigQueryClient = bigQueryClient;
            _options = options.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PubSubMessage message)
        {
            try
            {
                var data = Encoding.UTF8.GetString(Convert.FromBase64String(message.Message.Data));
                var json = JObject.Parse(data);
                var bucketName = json["bucket"]?.ToString();
                var objectName = json["name"]?.ToString();

                if (bucketName is null || objectName is null)
                {
                    _logger.LogError("Bucket or object name not found in Pub/Sub message.");
                    return BadRequest("Bucket or object name not found in Pub/Sub message.");
                }

                _logger.LogInformation($"Processing file: {objectName} from bucket: {bucketName}");

                using (var memoryStream = new MemoryStream())
                {
                    await _storageClient.DownloadObjectAsync(bucketName, objectName, memoryStream);
                    memoryStream.Position = 0;

                    var ionReader = IonReaderBuilder.Build(memoryStream);
                    var bigQueryTable = await _bigQueryClient.GetTableAsync(_options.DatasetId, _options.TableId);
                    var rows = new List<BigQueryInsertRow>();
                    var stringBuilder = new StringBuilder();
                    using (var stringWriter = new StringWriter(stringBuilder))
                    using (var ionWriter = IonTextWriterBuilder.Build(stringWriter))
                    {
                        ionWriter.WriteValues(ionReader);
                    }
                    rows.Add(new BigQueryInsertRow { { "data", stringBuilder.ToString() } });
                    await _bigQueryClient.InsertRowsAsync(bigQueryTable.Reference, rows);
                }

                _logger.LogInformation($"Successfully processed file: {objectName}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Pub/Sub message.");
                return new StatusCodeResult(500); // Nack
            }
        }
    }

    public class PubSubMessage
    {
        public required Message Message { get; set; }
    }

    public class Message
    {
        public required string Data { get; set; }
    }
}
