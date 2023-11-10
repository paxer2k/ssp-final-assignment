namespace Domain.Azure.QueueMessages
{
    public class GenerateImageLinksQueueMessage
    {
        public Dictionary<string, string> SasLinkPerStation { get; set; }
    }
}
