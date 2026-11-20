using System.Text.Json.Serialization;

namespace LifeManagementApp.Models;

public class WeatherResponse
{
    [JsonPropertyName("current")]
    public WeatherInfo? Current { get; set; }
}
