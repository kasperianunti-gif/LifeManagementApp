using LifeManagementApp.ViewModels;

namespace LifeManagementApp.Views;

public partial class TodoFormPage : ContentPage
{
    private readonly TodoFormViewModel viewModel;

    public TodoFormPage(TodoFormViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
