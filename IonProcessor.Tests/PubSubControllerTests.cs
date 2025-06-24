using System;
using System.Text;
using System.Threading.Tasks;
using IonProcessor.Controllers;
using IonProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IonProcessor.Tests
{
    public class PubSubControllerTests
    {
        private readonly Mock<IIonProcessingService> _mockIonProcessingService;
        private readonly Mock<IBigQueryService> _mockBigQueryService;
        private readonly Mock<ILogger<PubSubController>> _mockLogger;
        private readonly PubSubController _controller;

        public PubSubControllerTests()
        {
            _mockIonProcessingService = new Mock<IIonProcessingService>();
            _mockBigQueryService = new Mock<IBigQueryService>();
            _mockLogger = new Mock<ILogger<PubSubController>>();
            _controller = new PubSubController(
                _mockLogger.Object,
                _mockIonProcessingService.Object,
                _mockBigQueryService.Object);
        }

        [Fact]
        public async Task Post_WithValidMessage_ReturnsOk()
        {
            // Arrange
            var message = new PubSubMessage
            {
                Message = new Message { Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"bucket\":\"test-bucket\",\"name\":\"test-object\"}")) }
            };
            _mockIonProcessingService.Setup(s => s.ProcessIonFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("ion-data");

            // Act
            var result = await _controller.Post(message);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockIonProcessingService.Verify(s => s.ProcessIonFileAsync("test-bucket", "test-object"), Times.Once);
            _mockBigQueryService.Verify(s => s.InsertRowAsync("ion-data"), Times.Once);
        }

        [Fact]
        public async Task Post_WithMissingBucket_ReturnsBadRequest()
        {
            // Arrange
            var message = new PubSubMessage
            {
                Message = new Message { Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"name\":\"test-object\"}")) }
            };

            // Act
            var result = await _controller.Post(message);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Bucket or object name not found in Pub/Sub message.", badRequestResult.Value);
        }

        [Fact]
        public async Task Post_WithMissingObjectName_ReturnsBadRequest()
        {
            // Arrange
            var message = new PubSubMessage
            {
                Message = new Message { Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"bucket\":\"test-bucket\"}")) }
            };

            // Act
            var result = await _controller.Post(message);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Bucket or object name not found in Pub/Sub message.", badRequestResult.Value);
        }

        [Fact]
        public async Task Post_WhenIonProcessingFails_Returns500()
        {
            // Arrange
            var message = new PubSubMessage
            {
                Message = new Message { Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"bucket\":\"test-bucket\",\"name\":\"test-object\"}")) }
            };
            _mockIonProcessingService.Setup(s => s.ProcessIonFileAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("test exception"));

            // Act
            var result = await _controller.Post(message);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Post_WhenBigQueryFails_Returns500()
        {
            // Arrange
            var message = new PubSubMessage
            {
                Message = new Message { Data = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"bucket\":\"test-bucket\",\"name\":\"test-object\"}")) }
            };
            _mockIonProcessingService.Setup(s => s.ProcessIonFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("ion-data");
            _mockBigQueryService.Setup(s => s.InsertRowAsync(It.IsAny<string>())).ThrowsAsync(new Exception("test exception"));

            // Act
            var result = await _controller.Post(message);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}