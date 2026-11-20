using System.Net.Http.Json;
using LifeManagementApp.Models;

namespace LifeManagementApp.Services;

public class OpenMeteoWeatherService : IWeatherService
{
    private const string WeatherUrl =
        "https://api.open-meteo.com/v1/forecast?latitude=63.10&longitude=21.62" +
        "&current=temperature_2m,wind_speed_10m&wind_speed_unit=ms";

    private readonly HttpClient httpClient;

    public OpenMeteoWeatherService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<WeatherInfo> GetCurrentAsync()
    {
        var response = await httpClient.GetFromJsonAsync<WeatherResponse>(WeatherUrl);
        var weather = response?.Current;

        if (weather?.Temperature is null || weather.WindSpeed is null)
        {
            throw new InvalidDataException("Säävastauksesta puuttuu lämpötila tai tuulen nopeus.");
        }

        return weather;
    }
}
