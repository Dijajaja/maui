using AppTodoPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppTodoPro;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel viewModel;

    public LoginPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<LoginViewModel>();
        BindingContext = viewModel;
        viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private async void OnLoginSucceeded(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainTabs/MainPage");
    }

}
