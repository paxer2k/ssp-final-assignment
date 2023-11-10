using Domain.Entities;
using Service.Exceptions;
using Service.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Service.Images
{
    public class ImageService : IImageService
    {
        private readonly IUnsplashService _unsplashService;
        private readonly HttpClient _httpClient;

        public ImageService(IUnsplashService unsplashService, IHttpClientFactory httpClientFactory)
        {
            _unsplashService = unsplashService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<byte[]> GetImageBytesById(string id)
        {
            var photo = await _unsplashService.FetchPhotoByIdAsync(id);

            if (photo == null)
                throw new NotFoundException($"Photo with id {id} was not found!");

            var url = photo.Urls.Regular;

            return await _httpClient.GetByteArrayAsync(url);
        }

        public byte[] EditImage(byte[] imageInBytes, WeatherData weatherData)
        {
            var image = Image.Load(imageInBytes);

            var centerX = 50;
            var centerY = image.Height / 2;

            var text = new List<(string text, (float x, float y) position, int fontSize, string colorHex)>
            {
                ($"Stationd id: {weatherData.StationId}", (centerX, centerY - 220), 20, "#000000"),
                ($"Station name: {weatherData.StationName}", (centerX, centerY - 180), 20, "#000000"),
                ($"Region: {weatherData.Region}", (centerX, centerY - 140), 20, "#000000"),
                ($"Timestamp: {weatherData.TimeStamp}", (centerX, centerY - 100), 20, "#000000"),
                ($"Temperature: {weatherData.Temperature}°C", (centerX, centerY - 60), 20, "#000000"),
                ($"Humidity: {weatherData.Humidity}%", (centerX, centerY - 20), 20, "#000000"),
                ($"Wind speed: {weatherData.WindSpeed}km/h", (centerX, centerY + 20), 20, "#000000"),
                ($"Precipitation: {weatherData.Precipitation}%", (centerX, centerY + 60), 20, "#000000"),
                ($"Air pressure: {weatherData.AirPressure}atm", (centerX, centerY + 100), 20, "#000000"),
                ($"Description: {weatherData.WeatherDescription}", (centerX, centerY + 140), 20, "#000000")
            };

            return AddTextToImage(imageInBytes, text.ToArray());
        }

        private byte[] AddTextToImage(byte[] imageInBytes, params (string text, (float x, float y) position, int fontSize, string colorHex)[] texts)
        {
            var memoryStream = new MemoryStream();

            var image = Image.Load(imageInBytes);

            image.Clone(img =>
            {
                foreach (var (text, (x, y), fontSize, colorHex) in texts)
                {
                    var font = SystemFonts.CreateFont("Verdana", fontSize);
                    var color = Rgba32.ParseHex(colorHex);

                    img.DrawText(text, font, color, new PointF(x, y));
                }
            })
                .SaveAsJpeg(memoryStream);

            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }
    }
}
