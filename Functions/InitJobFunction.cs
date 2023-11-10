using Domain.Configuration.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Interfaces.Azure;
using System.Text.Json;
using System.Net;
using Domain.Enums;
using Domain.Azure.TableEntities;
using Domain.Azure.QueueMessages;

namespace Functions
{
    public class InitJobFunction
    {
        private readonly ILogger _logger;
        private readonly IWeatherDataService _weatherDataService;
        private readonly IImageService _imageService;
        private readonly IAppConfiguration _appConfiguration;
        private readonly ITableService _tableService;
        private readonly IQueueService _queueService;

        public InitJobFunction(ILoggerFactory loggerFactory, IWeatherDataService weatherDataService, IImageService imageService, IAppConfiguration appConfiguration, ITableService tableService, IQueueService queueService)
        {
            _logger = loggerFactory.CreateLogger<InitJobFunction>();
            _weatherDataService = weatherDataService;
            _imageService = imageService;
            _appConfiguration = appConfiguration;
            _tableService = tableService;
            _queueService = queueService;
        }

        [Function(nameof(InitJobFunction))]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            try
            {
                string jobId = Guid.NewGuid().ToString();

                var jobTableEntry = new JobTableEntity
                {
                    PartitionKey = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RowKey = jobId,
                    JobStatus = JobStatus.Processing
                };

                await _tableService.AddEntityAsync(jobTableEntry);

                var jobQueueMessage = new JobQueueMessage { JobId = jobId };

                await _queueService.SendMessageAsync(_appConfiguration.QueueConfig.InitJobQueue, JsonSerializer.Serialize(jobQueueMessage));

                await response.WriteStringAsync($"{_appConfiguration.JobConfig.ResultEndpoint}{jobId}");
                response.StatusCode = HttpStatusCode.OK;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}
