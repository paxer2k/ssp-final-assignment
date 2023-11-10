using Domain.Azure.TableEntities;
using Domain.Enums;
using Service.Interfaces;
using Service.Interfaces.Azure;

namespace Service.Jobs
{
    public class JobService : IJobService
    {
        private readonly ITableService _tableService;

        public JobService(ITableService tableService)
        {
            _tableService = tableService;
        }

        public JobStatus GetCurrentJobStatus(string jobId)
        {
            var tableEntry = _tableService.GetEntityByRowKey<JobTableEntity>(jobId);

            if (tableEntry == null)
                return JobStatus.None;

            return tableEntry.JobStatus;
        }
    }
}
