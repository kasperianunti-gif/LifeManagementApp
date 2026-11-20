using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LifeManagementApp.Models;
using LifeManagementApp.Services;
using Microsoft.Data.Sqlite;

namespace LifeManagementApp.ViewModels;

public partial class TodoListViewModel : ObservableObject
{
    private readonly ITodoRepository repository;
    private bool sortByDueDate;

    public ObservableCollection<TodoItem> Todos { get; } = new();

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private string sortText = "Järjestys: prioriteetti";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditTodoCommand))]
    private TodoItem? selectedTodo;

    public TodoListViewModel(ITodoRepository repository)
    {
        this.repository = repository;
    }

    [RelayCommand]
    private async Task LoadAsync(string? order)
    {
        if (order is not null)
        {
            sortByDueDate = order == "date";
        }

        ErrorMessage = "";

        try
        {
            var todos = await repository.GetAllAsync(sortByDueDate);
            SortText = sortByDueDate ? "Järjestys: eräpäivä" : "Järjestys: prioriteetti";
            SelectedTodo = null;
            Todos.Clear();

            foreach (var todo in todos)
            {
                Todos.Add(todo);
            }
        }
        catch (SqliteException)
        {
            ErrorMessage = "Tehtävälistan hakeminen epäonnistui. Kokeile lajittelupainiketta uudelleen.";
        }
    }

    [RelayCommand]
    private Task AddTodoAsync()
    {
        return Shell.Current.GoToAsync(AppShell.TodoFormRoute);
    }

    private bool CanEditTodo() => SelectedTodo is not null;

    [RelayCommand(CanExecute = nameof(CanEditTodo))]
    private Task EditTodoAsync()
    {
        if (SelectedTodo is null)
        {
            return Task.CompletedTask;
        }

        var parameters = new ShellNavigationQueryParameters
        {
            { "id", SelectedTodo.Id }
        };
        return Shell.Current.GoToAsync(AppShell.TodoFormRoute, parameters);
    }
}
