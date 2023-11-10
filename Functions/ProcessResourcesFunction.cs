using Domain.Azure.QueueMessages;
using Domain.Azure.TableEntities;
using Domain.Configuration.Interfaces;
using Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Interfaces.Azure;
using System.Text.Json;

namespace Functions
{
    public class ProcessResourcesFunction
    {
        private readonly ILogger<ProcessResourcesFunction> _logger;
        private readonly IWeatherDataService _weatherDataService;
        private readonly IImageService _imageService;
        private readonly IAppConfiguration _appConfiguration;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;

        public ProcessResourcesFunction(ILogger<ProcessResourcesFunction> logger, IWeatherDataService weatherDataService, IImageService imageService, IAppConfiguration appConfiguration, IBlobService blobService, IQueueService queueService, ITableService tableService)
        {
            _logger = logger;
            _weatherDataService = weatherDataService;
            _imageService = imageService;
            _appConfiguration = appConfiguration;
            _blobService = blobService;
            _queueService = queueService;
            _tableService = tableService;
        }

        [Function(nameof(ProcessResourcesFunction))]
        public async Task Run([QueueTrigger("init-job-queue")] JobQueueMessage message)
        {
            var jobTableEntry = _tableService.GetEntityByRowKey<JobTableEntity>(message.JobId);

            try
            {
                _logger.LogInformation($"Processing resources...");

                _logger.LogInformation($"Fetching weather data...");
                var weatherData = await _weatherDataService.GetWeatherDataAsync();

                _logger.LogInformation($"Fetching original image...");
                var imageInBytes = await _imageService.GetImageBytesById(_appConfiguration.UnsplashConfig.PhotoId);

                var processResourcesQueueMessage = new ProcessResourcesQueueMessage
                {
                    JobId = message.JobId,
                    WeatherDataCollection = weatherData.ToList(),
                };

                _logger.LogInformation($"Sending weather information to the queue...");
                await _queueService.SendMessageAsync(_appConfiguration.QueueConfig.ProcessResourcesQueue, JsonSerializer.Serialize(processResourcesQueueMessage));

                _logger.LogInformation($"Uploading original image to blob...");
                await _blobService.UploadAsync(_appConfiguration.BlobConfig.UploadImageContainer, imageInBytes, _appConfiguration.BlobConfig.OriginalImageFilename);
            }
            catch (Exception ex)
            {
                jobTableEntry.JobStatus = JobStatus.Failed;
                await _tableService.UpdateEntityAsync(jobTableEntry);

                _logger.LogError($"Exception: {ex.Message}");
            }
        }
    }
}
