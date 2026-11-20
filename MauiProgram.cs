using LifeManagementApp.Data;
using LifeManagementApp.Services;
using LifeManagementApp.ViewModels;
using LifeManagementApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LifeManagementApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        string databasePath = Path.Combine(FileSystem.AppDataDirectory, "todos.db");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        builder.Services.AddSingleton(new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        });
        builder.Services.AddSingleton<IWeatherService, OpenMeteoWeatherService>();
        builder.Services.AddSingleton<ITodoRepository, TodoRepository>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<TodoListPage>();
        builder.Services.AddSingleton<TodoListViewModel>();
        builder.Services.AddTransient<TodoFormPage>();
        builder.Services.AddTransient<TodoFormViewModel>();

        return builder.Build();
    }
}
