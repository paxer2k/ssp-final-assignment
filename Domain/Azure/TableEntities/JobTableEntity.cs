using Azure;
using Azure.Data.Tables;
using Domain.Enums;

namespace Domain.Azure.TableEntities
{
    public class JobTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
