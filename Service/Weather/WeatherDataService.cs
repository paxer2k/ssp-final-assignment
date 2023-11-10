using Domain.Configuration.Interfaces;
using Domain.Entities;
using Newtonsoft.Json.Linq;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Weather
{
    public class WeatherDataService : IWeatherDataService
    {
        private readonly IAppConfiguration _appConfiguration;
        private readonly HttpClient _httpClient;

        public WeatherDataService(IAppConfiguration appConfiguration, IHttpClientFactory httpClientFactory)
        {
            _appConfiguration = appConfiguration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<WeatherData>> GetWeatherDataAsync()
        {
            string json = await _httpClient.GetStringAsync(_appConfiguration.BuienradarConfig.BuienradarDataUrl);

            JObject data = JObject.Parse(json);

            if (data["actual"] == null || data["actual"]["stationmeasurements"] == null)
                throw new BadRequestException("The weather data is faulty, please check it out.");

            JArray stations = (JArray)data["actual"]["stationmeasurements"];

            var weatherDataList = stations.Select(station => new WeatherData
            {
                StationId = station["stationid"]?.Value<int>() ?? 0,
                StationName = station["stationname"]?.ToString(),
                Region = station["regio"]?.ToString(),
                TimeStamp = station["timestamp"]?.Value<DateTime>() ?? DateTime.MinValue,
                Temperature = station["temperature"]?.Value<double>() ?? 0.0,
                Humidity = station["humidity"]?.Value<int>() ?? 0,
                WindSpeed = station["windspeed"]?.Value<double>() ?? 0.0,
                Precipitation = station["precipitation"]?.Value<double>() ?? 0.0,
                AirPressure = station["airpressure"]?.Value<double>() ?? 0.0,
                WeatherDescription = station["weatherdescription"]?.ToString()

            }).ToList();

            return weatherDataList;
        }
    }
}