using System.Text.Json.Serialization;

namespace LifeManagementApp.Models;

public class WeatherInfo
{
    [JsonPropertyName("temperature_2m")]
    public double? Temperature { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double? WindSpeed { get; set; }
}
