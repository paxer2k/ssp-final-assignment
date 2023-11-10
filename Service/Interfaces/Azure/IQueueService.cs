using Azure.Storage.Queues.Models;
using System.Globalization;

namespace Service.Interfaces.Azure
{
    public interface IQueueService
    {
        Task SendMessageAsync(string queueName, string message);
    }
}
