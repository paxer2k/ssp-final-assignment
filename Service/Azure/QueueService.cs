using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Domain.Configuration.Interfaces;
using Service.Interfaces.Azure;

namespace Service.Azure
{
    public class QueueService : IQueueService
    {
        private readonly IAppConfiguration _appConfiguration;
        private readonly QueueServiceClient _queueServiceClient;
        public QueueService(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;

            var options = new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            };

            _queueServiceClient = new QueueServiceClient(_appConfiguration.QueueConfig.QueueConnectionString, options: options);
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            QueueClient queueClient = await _queueServiceClient.CreateQueueAsync(queueName);

            await queueClient.CreateIfNotExistsAsync();

            await queueClient.SendMessageAsync(message);
        }
    }
}
