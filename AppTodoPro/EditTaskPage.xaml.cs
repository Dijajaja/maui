using AppTodoPro.Models;
using AppTodoPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppTodoPro;

public partial class EditTaskPage : ContentPage
{
    private readonly EditTaskViewModel viewModel;

    public EditTaskPage(TodoItem item)
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<EditTaskViewModel>();
        viewModel.Initialize(item);
        BindingContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private async void OnCloseRequested(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
