using LifeManagementApp.Data;
using LifeManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeManagementApp.Services;

public class TodoRepository : ITodoRepository
{
    private readonly IDbContextFactory<AppDbContext> databaseFactory;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool databaseReady;

    public TodoRepository(IDbContextFactory<AppDbContext> databaseFactory)
    {
        this.databaseFactory = databaseFactory;
    }

    public async Task<List<TodoItem>> GetTopTodosAsync()
    {
        await using var database = await databaseFactory.CreateDbContextAsync();
        await EnsureDatabaseAsync(database);

        var unfinished = database.Todos.AsNoTracking().Where(todo => !todo.IsDone);
        return await OrderByPriority(unfinished).Take(3).ToListAsync();
    }

    public async Task<List<TodoItem>> GetAllAsync(bool sortByDueDate)
    {
        await using var database = await databaseFactory.CreateDbContextAsync();
        await EnsureDatabaseAsync(database);

        var todos = database.Todos.AsNoTracking();
        if (sortByDueDate)
        {
            return await todos.OrderBy(todo => todo.DueDate)
                              .ThenByDescending(todo => todo.Priority)
                              .ThenBy(todo => todo.Id)
                              .ToListAsync();
        }

        return await OrderByPriority(todos).ToListAsync();
    }

    public async Task<TodoItem?> FindAsync(int id)
    {
        await using var database = await databaseFactory.CreateDbContextAsync();
        await EnsureDatabaseAsync(database);

        return await database.Todos.AsNoTracking().SingleOrDefaultAsync(todo => todo.Id == id);
    }

    public async Task<bool> SaveAsync(TodoItem todo)
    {
        await using var database = await databaseFactory.CreateDbContextAsync();
        await EnsureDatabaseAsync(database);

        if (todo.Id == 0)
        {
            database.Todos.Add(todo);
        }
        else
        {
            var savedTodo = await database.Todos.FindAsync(todo.Id);
            if (savedTodo is null)
            {
                return false;
            }

            savedTodo.Title = todo.Title;
            savedTodo.Priority = todo.Priority;
            savedTodo.DueDate = todo.DueDate;
            savedTodo.IsDone = todo.IsDone;
        }

        await database.SaveChangesAsync();
        return true;
    }

    private static IOrderedQueryable<TodoItem> OrderByPriority(IQueryable<TodoItem> todos)
    {
        return todos.OrderByDescending(todo => todo.Priority)
                    .ThenBy(todo => todo.DueDate)
                    .ThenBy(todo => todo.Id);
    }

    private async Task EnsureDatabaseAsync(AppDbContext database)
    {
        await initializationLock.WaitAsync();
        try
        {
            if (!databaseReady)
            {
                await database.Database.EnsureCreatedAsync();
                databaseReady = true;
            }
        }
        finally
        {
            initializationLock.Release();
        }
    }
}
