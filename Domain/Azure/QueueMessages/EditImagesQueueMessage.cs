namespace Domain.Azure.QueueMessages
{
    public class EditImagesQueueMessage
    {
        public string JobId { get; set; }
        public Dictionary<string, string> StationPerLink { get; set; }
    }
}
