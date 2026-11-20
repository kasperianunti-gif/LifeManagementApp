using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifeManagementApp.Models;
using LifeManagementApp.Services;
using Microsoft.Data.Sqlite;

namespace LifeManagementApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private const double KitesurfWindLimit = 5;
    private readonly ITodoRepository repository;
    private readonly IWeatherService weatherService;

    public ObservableCollection<TodoItem> TopTodos { get; } = new();

    [ObservableProperty]
    private string todoError = "";

    [ObservableProperty]
    private string weatherText = "";

    [ObservableProperty]
    private string weatherError = "";

    [ObservableProperty]
    private bool isWeatherLoading;

    [ObservableProperty]
    private bool showKitesurfNotice;

    public HomeViewModel(ITodoRepository repository, IWeatherService weatherService)
    {
        this.repository = repository;
        this.weatherService = weatherService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await Task.WhenAll(LoadTopTodosAsync(), LoadWeatherAsync());
    }

    private async Task LoadTopTodosAsync()
    {
        TodoError = "";
        try
        {
            var todos = await repository.GetTopTodosAsync();
            TopTodos.Clear();

            foreach (var todo in todos)
            {
                TopTodos.Add(todo);
            }
        }
        catch (SqliteException)
        {
            TodoError = "Tärkeimpien tehtävien hakeminen epäonnistui.";
        }
    }

    private async Task LoadWeatherAsync()
    {
        IsWeatherLoading = true;
        WeatherText = "";
        WeatherError = "";
        ShowKitesurfNotice = false;

        try
        {
            var weather = await weatherService.GetCurrentAsync();
            WeatherText = $"Lämpötila {weather.Temperature:0.0} °C · Tuuli {weather.WindSpeed:0.0} m/s";
            ShowKitesurfNotice = weather.WindSpeed > KitesurfWindLimit;
        }
        catch (TaskCanceledException)
        {
            WeatherError = "Säähaku aikakatkaistiin. Kokeile päivittää uudelleen.";
        }
        catch (HttpRequestException)
        {
            WeatherError = "Säätä ei voitu hakea. Tarkista verkkoyhteys ja yritä uudelleen.";
        }
        catch (Exception error) when (error is IOException or InvalidDataException or NotSupportedException)
        {
            WeatherError = "Sääpalvelun vastausta ei voitu käyttää.";
        }
        catch (JsonException)
        {
            WeatherError = "Sääpalvelun vastausta ei voitu lukea.";
        }
        finally
        {
            IsWeatherLoading = false;
        }
    }
}
