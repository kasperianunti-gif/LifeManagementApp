using LifeManagementApp.Models;

namespace LifeManagementApp.Services;

public interface ITodoRepository
{
    Task<List<TodoItem>> GetTopTodosAsync();
    Task<List<TodoItem>> GetAllAsync(bool sortByDueDate);
    Task<TodoItem?> FindAsync(int id);
    Task<bool> SaveAsync(TodoItem todo);
}
