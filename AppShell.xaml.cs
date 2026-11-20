using LifeManagementApp.Views;

namespace LifeManagementApp;

public partial class AppShell : Shell
{
    public const string TodoFormRoute = "todo-form";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(TodoFormRoute, typeof(TodoFormPage));
    }
}
