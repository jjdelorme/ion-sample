using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IonProcessor.Services
{
    public class BigQueryService : IBigQueryService
    {
        private readonly BigQueryClient _bigQueryClient;
        private readonly GoogleCloudOptions _options;
        private readonly ILogger<BigQueryService> _logger;

        public BigQueryService(BigQueryClient bigQueryClient, IOptions<GoogleCloudOptions> options, ILogger<BigQueryService> logger)
        {
            _bigQueryClient = bigQueryClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InsertRowAsync(string data)
        {
            var bigQueryTable = await _bigQueryClient.GetTableAsync(_options.DatasetId, _options.TableId);
            var rows = new List<BigQueryInsertRow>
            {
                new BigQueryInsertRow { { "data", data } }
            };
            await _bigQueryClient.InsertRowsAsync(bigQueryTable.Reference, rows);
            _logger.LogInformation("Successfully inserted row into BigQuery.");
        }
    }
}
