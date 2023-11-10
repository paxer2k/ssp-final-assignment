using Domain.Configuration.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Domain.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public BuienradarConfig BuienradarConfig { get; set; }
        public UnsplashConfig UnsplashConfig { get; set; }
        public BlobConfig BlobConfig { get; set; }
        public TableConfig TableConfig { get; set; }
        public QueueConfig QueueConfig { get; set; }
        public JobConfig JobConfig { get; set; }

        public AppConfiguration(IConfiguration configuration)
        {
            BuienradarConfig = new BuienradarConfig
            {
                BuienradarDataUrl = configuration.GetSection("BuienradarConfig:BuienradarDataUrl").Value
            };

            UnsplashConfig = new UnsplashConfig
            {
                AccessKey = configuration.GetSection("UnsplashConfig:AccessKey").Value,
                PhotoId = configuration.GetSection("UnsplashConfig:PhotoId").Value,     
            };

            JobConfig = new JobConfig
            {
                ResultEndpoint = configuration.GetSection("JobConfig:ResultEndpoint").Value,
            };

            BlobConfig = new BlobConfig
            {
                BlobConnectionString = configuration.GetSection("AzureWebJobsStorage").Value,
                UploadImageContainer = configuration.GetSection("BlobConfig:UploadImageContainer").Value,
                SasLinkContainer = configuration.GetSection("BlobConfig:SasLinkContainer").Value,
                OriginalImageFilename = configuration.GetSection("BlobConfig:OriginalImageFilename").Value
            };

            QueueConfig = new QueueConfig
            {
                QueueConnectionString = configuration.GetSection("AzureWebJobsStorage").Value,
                InitJobQueue = configuration.GetSection("QueueConfig:InitJobQueue").Value,
                ProcessResourcesQueue = configuration.GetSection("QueueConfig:ProcessResourcesQueue").Value,
                EditImagesQueue = configuration.GetSection("QueueConfig:EditImagesQueue").Value,
            };

            TableConfig = new TableConfig
            {
                TableConnectionString = configuration.GetSection("AzureWebJobsStorage").Value,
                JobTable = configuration.GetSection("TableConfig:JobTable").Value
            };
        }
    }

    public class BuienradarConfig
    {
        public string BuienradarDataUrl { get; internal set; } = string.Empty;
    }

    public class UnsplashConfig
    {
        public string AccessKey { get; internal set; } = string.Empty;
        public string PhotoId { get; internal set; } = string.Empty;
    }

    public class BlobConfig
    {
        public string BlobConnectionString { get; internal set; } = string.Empty;
        public string UploadImageContainer { get; internal set; } = string.Empty;
        public string SasLinkContainer { get; internal set; } = string.Empty;
        public string OriginalImageFilename { get; internal set; } = string.Empty;
    }

    public class TableConfig
    {
        public string TableConnectionString { get; internal set; } = string.Empty;
        public string JobTable { get; internal set; } = string.Empty;
    }

    public class QueueConfig
    {
        public string QueueConnectionString { get; internal set; } = string.Empty;
        public string InitJobQueue { get; internal set; } = string.Empty;
        public string ProcessResourcesQueue { get; internal set; } = string.Empty;
        public string EditImagesQueue { get; internal set; } = string.Empty;
    }

    public class JobConfig
    {
        public string ResultEndpoint { get; internal set; } = string.Empty;
    }
}
