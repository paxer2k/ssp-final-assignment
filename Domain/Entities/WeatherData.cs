namespace Domain.Entities
{
    public class WeatherData
    {
        public int StationId { get; set; }
        public string? StationName { get; set; }
        public string? Region { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double Precipitation { get; set; }
        public double AirPressure { get; set; }
        public string? WeatherDescription { get; set; }
    }
}