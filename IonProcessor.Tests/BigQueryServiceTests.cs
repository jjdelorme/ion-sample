using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using IonProcessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IonProcessor.Tests
{
    public class BigQueryServiceTests
    {
        private readonly Mock<BigQueryClient> _mockBigQueryClient;
        private readonly Mock<IOptions<GoogleCloudOptions>> _mockOptions;
        private readonly Mock<ILogger<BigQueryService>> _mockLogger;
        private readonly BigQueryService _service;

        public BigQueryServiceTests()
        {
            _mockBigQueryClient = new Mock<BigQueryClient>();
            _mockOptions = new Mock<IOptions<GoogleCloudOptions>>();
            _mockLogger = new Mock<ILogger<BigQueryService>>();

            _mockOptions.Setup(o => o.Value).Returns(new GoogleCloudOptions
            {
                ProjectId = "test-project",
                DatasetId = "test-dataset",
                TableId = "test-table"
            });

            _service = new BigQueryService(_mockBigQueryClient.Object, _mockOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InsertRowAsync_WithValidData_CallsBigQueryClient()
        {
            // Arrange
            var data = "ion-data";
            var tableReference = new TableReference { ProjectId = "test-project", DatasetId = "test-dataset", TableId = "test-table" };
            var bigQueryTable = new BigQueryTable(_mockBigQueryClient.Object, new Table { TableReference = tableReference });

            _mockBigQueryClient.Setup(c => c.GetTableAsync("test-dataset", "test-table", It.IsAny<GetTableOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bigQueryTable);

            // Act
            await _service.InsertRowAsync(data);

            // Assert
            _mockBigQueryClient.Verify(c => c.InsertRowsAsync(tableReference, It.IsAny<IEnumerable<BigQueryInsertRow>>(), It.IsAny<InsertOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InsertRowAsync_WhenBigQueryFails_ThrowsException()
        {
            // Arrange
            var data = "ion-data";
            var tableReference = new TableReference { ProjectId = "test-project", DatasetId = "test-dataset", TableId = "test-table" };
            var bigQueryTable = new BigQueryTable(_mockBigQueryClient.Object, new Table { TableReference = tableReference });

            _mockBigQueryClient.Setup(c => c.GetTableAsync("test-dataset", "test-table", It.IsAny<GetTableOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bigQueryTable);
            _mockBigQueryClient.Setup(c => c.InsertRowsAsync(tableReference, It.IsAny<IEnumerable<BigQueryInsertRow>>(), It.IsAny<InsertOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("BigQuery failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.InsertRowAsync(data));
        }
    }
}
