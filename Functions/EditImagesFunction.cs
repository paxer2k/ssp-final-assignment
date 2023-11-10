using Domain.Azure.QueueMessages;
using Domain.Azure.TableEntities;
using Domain.Configuration.Interfaces;
using Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Service.Azure;
using Service.Interfaces;
using Service.Interfaces.Azure;
using System.Text.Json;

namespace Functions
{
    public class EditImagesFunction
    {
        private readonly ILogger<EditImagesFunction> _logger;
        private readonly IImageService _imageService;
        private readonly IBlobService _blobService;
        private readonly IAppConfiguration _appConfiguration;
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;

        public EditImagesFunction(ILogger<EditImagesFunction> logger, IImageService imageService, IBlobService blobService, IAppConfiguration appConfiguration, IQueueService queueService, ITableService tableService)
        {
            _logger = logger;
            _imageService = imageService;
            _blobService = blobService;
            _appConfiguration = appConfiguration;
            _queueService = queueService;
            _tableService = tableService;
        }

        [Function(nameof(EditImagesFunction))]
        public async Task Run([QueueTrigger("process-resources-queue")] ProcessResourcesQueueMessage message)
        {
            var jobTableEntry = _tableService.GetEntityByRowKey<JobTableEntity>(message.JobId);

            try
            {
                _logger.LogInformation($"Processing queue job for applying text to images for: {message.JobId} ");

                var originalImage = await _blobService.DownloadAsync(_appConfiguration.BlobConfig.UploadImageContainer, _appConfiguration.BlobConfig.OriginalImageFilename);

                Dictionary<string, string> stationPerLink = new Dictionary<string, string>();

                foreach (var weather in message.WeatherDataCollection)
                {
                    var editedImage = _imageService.EditImage(originalImage, weather);

                    var fileName = $"{message.JobId}{weather.StationId}";

                    await _blobService.UploadAsync(_appConfiguration.BlobConfig.UploadImageContainer, editedImage, fileName);

                    stationPerLink.Add($"{weather.StationId}-{weather.StationName}", fileName);

                    _logger.LogInformation($"Image has been processed and uploaded: {fileName}");
                }

                await _blobService.DeleteAsync(_appConfiguration.BlobConfig.UploadImageContainer, _appConfiguration.BlobConfig.OriginalImageFilename); // no need anymore

                var editImageQueueMessage = new EditImagesQueueMessage
                {
                    JobId = message.JobId,
                    StationPerLink = stationPerLink,
                };

                _logger.LogInformation($"Storing all file names in queue...");
                await _queueService.SendMessageAsync(_appConfiguration.QueueConfig.EditImagesQueue, JsonSerializer.Serialize(editImageQueueMessage));

                _logger.LogInformation($"Original image deleted...");
                _logger.LogInformation($"Processing has finished...");
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
