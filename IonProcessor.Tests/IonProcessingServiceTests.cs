using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Download;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using IonProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace IonProcessor.Tests
{
    public class IonProcessingServiceTests
    {
        private readonly Mock<StorageClient> _mockStorageClient;
        private readonly Mock<ILogger<IonProcessingService>> _mockLogger;
        private readonly IonProcessingService _service;

        public IonProcessingServiceTests()
        {
            _mockStorageClient = new Mock<StorageClient>();
            _mockLogger = new Mock<ILogger<IonProcessingService>>();
            _service = new IonProcessingService(_mockStorageClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessIonFileAsync_WithValidFile_ReturnsIonDataAsString()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-object";
            var ionData = "hello";
            var ionBytes = Encoding.UTF8.GetBytes(ionData);
            var memoryStream = new MemoryStream(ionBytes);
            memoryStream.Position = 0;

            _mockStorageClient.Setup(s => s.DownloadObjectAsync(bucketName, objectName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<IDownloadProgress>>()))
                .Callback<string, string, Stream, DownloadObjectOptions, CancellationToken, IProgress<IDownloadProgress>>((b, o, s, opt, ct, p) =>
                {
                    memoryStream.CopyTo(s);
                    s.Position = 0;
                })
                .Returns(Task.FromResult(new Object()));


            // Act
            var result = await _service.ProcessIonFileAsync(bucketName, objectName);

            // Assert
            Assert.Equal(ionData, result.Trim());
        }

        [Fact]
        public async Task ProcessIonFileAsync_WhenDownloadFails_ThrowsException()
        {
            // Arrange
            var bucketName = "test-bucket";
            var objectName = "test-object";
            _mockStorageClient.Setup(s => s.DownloadObjectAsync(bucketName, objectName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<IDownloadProgress>>()))
                .ThrowsAsync(new Exception("Download failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessIonFileAsync(bucketName, objectName));
        }
    }
}
