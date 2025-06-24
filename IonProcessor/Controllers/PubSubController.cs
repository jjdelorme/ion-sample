using System;
using System.Text;
using System.Threading.Tasks;
using IonProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IonProcessor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PubSubController : ControllerBase
    {
        private readonly ILogger<PubSubController> _logger;
        private readonly IIonProcessingService _ionProcessingService;
        private readonly IBigQueryService _bigQueryService;

        public PubSubController(
            ILogger<PubSubController> logger,
            IIonProcessingService ionProcessingService,
            IBigQueryService bigQueryService)
        {
            _logger = logger;
            _ionProcessingService = ionProcessingService;
            _bigQueryService = bigQueryService;
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

                var ionData = await _ionProcessingService.ProcessIonFileAsync(bucketName, objectName);
                await _bigQueryService.InsertRowAsync(ionData);

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
