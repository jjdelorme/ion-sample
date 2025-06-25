using System;
using System.Text;
using System.Threading.Tasks;
using IonProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
                // Add Debug log for the whole message
                _logger.LogDebug($"Received Pub/Sub message: {JsonSerializer.Serialize(message)}");

                var data = Encoding.UTF8.GetString(Convert.FromBase64String(message.Message.Data));
                using var json = JsonDocument.Parse(data);
                if (!json.RootElement.TryGetProperty("bucket", out var bucketElement) ||
                    !json.RootElement.TryGetProperty("name", out var nameElement))
                {
                    _logger.LogError("Bucket or object name not found in Pub/Sub message.");
                    return BadRequest("Bucket or object name not found in Pub/Sub message.");
                }

                var bucketName = bucketElement.GetString();
                var objectName = nameElement.GetString();

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
