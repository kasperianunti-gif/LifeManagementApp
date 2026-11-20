using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifeManagementApp.Models;
using LifeManagementApp.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LifeManagementApp.ViewModels;

public partial class TodoFormViewModel : ObservableObject, IQueryAttributable
{
    private readonly ITodoRepository repository;
    private int todoId;

    public int[] Priorities { get; } = [1, 2, 3, 4, 5];

    [ObservableProperty]
    private string pageTitle = "Uusi tehtävä";

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private int priority = 3;

    [ObservableProperty]
    private DateTime dueDate = DateTime.Today;

    [ObservableProperty]
    private bool isDone;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isReady;

    public TodoFormViewModel(ITodoRepository repository)
    {
        this.repository = repository;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && value is int id)
        {
            todoId = id;
            PageTitle = "Muokkaa tehtävää";
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (todoId == 0)
        {
            IsReady = true;
            return;
        }

        ErrorMessage = "";

        try
        {
            var todo = await repository.FindAsync(todoId);
            if (todo is null)
            {
                ErrorMessage = "Tehtävää ei löytynyt.";
                return;
            }

            Title = todo.Title;
            Priority = todo.Priority;
            DueDate = todo.DueDate;
            IsDone = todo.IsDone;
            IsReady = true;
        }
        catch (SqliteException)
        {
            ErrorMessage = "Tehtävän avaaminen epäonnistui.";
        }
    }

    private bool CanSave() => IsReady;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Anna tehtävälle otsikko.";
            return;
        }

        var todo = new TodoItem
        {
            Id = todoId,
            Title = Title.Trim(),
            Priority = Priority,
            DueDate = DueDate.Date,
            IsDone = IsDone
        };

        try
        {
            if (!await repository.SaveAsync(todo))
            {
                ErrorMessage = "Päivitettävää tehtävää ei löytynyt.";
                return;
            }
        }
        catch (Exception error) when (error is DbUpdateException or SqliteException)
        {
            ErrorMessage = "Tallennus epäonnistui. Yritä uudelleen.";
            return;
        }

        IsReady = false;
        await Shell.Current.GoToAsync("..");
    }
}
