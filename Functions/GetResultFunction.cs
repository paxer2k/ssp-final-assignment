using Domain.Azure.QueueMessages;
using Domain.Configuration.Interfaces;
using Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Interfaces;
using Service.Interfaces.Azure;
using System.Net;
using System.Text;

namespace Functions
{
    public class GetResultFunction
    {
        private readonly ILogger _logger;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly IAppConfiguration _appConfiguration;
        private readonly IJobService _jobService;

        public GetResultFunction(ILoggerFactory loggerFactory, IBlobService blobService, IQueueService queueService, IAppConfiguration appConfiguration, IJobService jobService)
        {
            _logger = loggerFactory.CreateLogger<GetResultFunction>();
            _blobService = blobService;
            _queueService = queueService;
            _appConfiguration = appConfiguration;
            _jobService = jobService;
        }

        [Function(nameof(GetResultFunction))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetResultFunction/{jobId}")] HttpRequestData req, string jobId)
        {
            var response = req.CreateResponse();

            try
            {
                var status = _jobService.GetCurrentJobStatus(jobId);

                switch (status)
                {
                    case JobStatus.None:
                        return GetResponse(response, $"Status: {status} - This job id is invalid or has no processes associated with it.", HttpStatusCode.NotFound);
                    case JobStatus.Processing:
                        return GetResponse(response, $"Status: {status} - This job is still being processed, please wait a little longer...", HttpStatusCode.OK);
                    case JobStatus.Failed:
                        return GetResponse(response, $"Status: {status} - This job has failed. Please start a new job.", HttpStatusCode.OK);
                    case JobStatus.Complete:
                        return await ProcessCompleteJob(response, jobId, status);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.WriteString("Something went wrong... This job is being terminated. Please start a new one");
                _logger.LogError($"Exception: {ex.Message}");
            }

            return response;
        }

        private HttpResponseData GetResponse(HttpResponseData response, string message, HttpStatusCode statusCode)
        {
            response.StatusCode = statusCode;
            response.WriteString(message);
            return response;
        }

        private async Task<HttpResponseData> ProcessCompleteJob(HttpResponseData response, string jobId, JobStatus status)
        {
            var content = await _blobService.DownloadAsync(_appConfiguration.BlobConfig.SasLinkContainer, $"{jobId}.json");
            var jsonString = Encoding.UTF8.GetString(content);
            var sasLinksPerStation = JsonConvert.DeserializeObject<GenerateImageLinksQueueMessage>(jsonString)!.SasLinkPerStation;

            response.WriteString($"Status: {status} - The job has been completed successfully, here are the SAS links to the images:\n");
            foreach (var entry in sasLinksPerStation)
            {
                response.WriteString($"Weather data for station {entry.Key} | {entry.Value}\n");
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}
