using Domain.Azure.QueueMessages;
using Domain.Azure.TableEntities;
using Domain.Configuration.Interfaces;
using Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Interfaces.Azure;
using System.Text;

namespace Functions
{
    public class GenerateImageLinksFunction
    {
        private readonly ILogger _logger;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly IAppConfiguration _appConfiguration;
        private readonly ITableService _tableService;

        public GenerateImageLinksFunction(ILoggerFactory loggerFactory, IBlobService blobService, IQueueService queueService, IAppConfiguration appConfiguration, ITableService tableService)
        {
            _logger = loggerFactory.CreateLogger<GenerateImageLinksFunction>();
            _blobService = blobService;
            _queueService = queueService;
            _appConfiguration = appConfiguration;
            _tableService = tableService;
        }

        [Function(nameof(GenerateImageLinksFunction))]
        public async Task Run([QueueTrigger("edit-images-queue")] EditImagesQueueMessage message)
        {
            var jobTableEntry = _tableService.GetEntityByRowKey<JobTableEntity>(message.JobId);

            try
            {
                _logger.LogInformation($"C# Queue trigger function processed:");

                var sasLinkPerStation = message.StationPerLink
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => _blobService.GenerateSasLink(_appConfiguration.BlobConfig.UploadImageContainer, kvp.Value)
                    ); 

                var generateImageLinksQueueMessage = new GenerateImageLinksQueueMessage
                {
                    SasLinkPerStation = sasLinkPerStation
                };

                var serializedQueueMessage = JsonConvert.SerializeObject(generateImageLinksQueueMessage);
                var encodedQueueMessage = Encoding.UTF8.GetBytes(serializedQueueMessage);

                var fileName = $"{message.JobId}.json";

                await _blobService.UploadAsync(_appConfiguration.BlobConfig.SasLinkContainer, encodedQueueMessage, fileName);
                
                jobTableEntry.JobStatus = JobStatus.Complete;
                await _tableService.UpdateEntityAsync(jobTableEntry); // FINISHED, SO UPDATE TABLE STORAGE 
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
