namespace Domain.Configuration.Interfaces
{
    public interface IAppConfiguration
    {
        BuienradarConfig BuienradarConfig { get; }
        UnsplashConfig UnsplashConfig { get; }
        BlobConfig BlobConfig { get; }
        TableConfig TableConfig { get; }
        QueueConfig QueueConfig { get; }
        JobConfig JobConfig { get; }
    }
}
