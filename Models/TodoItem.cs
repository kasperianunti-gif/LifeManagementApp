namespace LifeManagementApp.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsDone { get; set; }
}
