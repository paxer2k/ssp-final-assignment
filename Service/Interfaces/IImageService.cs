using Domain.Entities;

namespace Service.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> GetImageBytesById(string id);
        byte[] EditImage(byte[] imageInByes, WeatherData weatherData);
    }
}
