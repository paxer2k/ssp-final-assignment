using Domain.Enums;

namespace Service.Interfaces
{
    public interface IJobService
    {
        JobStatus GetCurrentJobStatus(string jobId);
    }
}
