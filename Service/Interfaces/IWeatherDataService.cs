using Domain.Entities;

namespace Service.Interfaces
{
    public interface IWeatherDataService
    {
        Task<IEnumerable<WeatherData>> GetWeatherDataAsync();
    }
}
