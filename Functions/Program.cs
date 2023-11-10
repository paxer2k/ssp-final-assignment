using Domain.Configuration;
using Domain.Configuration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service;
using Service.Azure;
using Service.Images;
using Service.Interfaces;
using Service.Interfaces.Azure;
using Service.Jobs;
using Service.Weather;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddSingleton<IAppConfiguration, AppConfiguration>();
        services.AddSingleton<IUnsplashService, UnsplashService>();
        services.AddScoped<IWeatherDataService, WeatherDataService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IQueueService, QueueService>();
    })
    .Build();


host.Run();
