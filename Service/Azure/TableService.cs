using Azure;
using Azure.Data.Tables;
using Domain.Azure.TableEntities;
using Domain.Configuration.Interfaces;
using Service.Interfaces.Azure;

namespace Service.Azure
{
    public class TableService : ITableService
    {
        private readonly IAppConfiguration _appConfiguration;
        private readonly TableServiceClient _tableServiceClient;

        public TableService(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
            _tableServiceClient = new TableServiceClient(_appConfiguration.TableConfig.TableConnectionString);
        }

        public async Task AddEntityAsync<T>(T item) where T : ITableEntity
        {
            TableClient tableClient = _tableServiceClient.GetTableClient(_appConfiguration.TableConfig.JobTable);

            await tableClient.CreateIfNotExistsAsync();

            await tableClient.AddEntityAsync(item);
        }

        public async Task UpdateEntityAsync<T>(T item) where T : ITableEntity
        {
            TableClient tableClient = _tableServiceClient.GetTableClient(_appConfiguration.TableConfig.JobTable);

            await tableClient.UpdateEntityAsync(item, ETag.All);
        }

        public JobTableEntity GetEntityByRowKey<T>(string rowKey) where T : ITableEntity
        {
            TableClient tableClient = _tableServiceClient.GetTableClient(_appConfiguration.TableConfig.JobTable);

            var query = tableClient.Query<JobTableEntity>(c => c.RowKey == rowKey).FirstOrDefault();

            if (query == null)
                return null;

            return query;
        }
    }
}
