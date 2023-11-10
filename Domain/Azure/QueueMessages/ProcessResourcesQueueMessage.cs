using Domain.Entities;

namespace Domain.Azure.QueueMessages
{
    public class ProcessResourcesQueueMessage
    {
        public string JobId { get; set; }
        public List<WeatherData> WeatherDataCollection { get; set; }
    }
}
