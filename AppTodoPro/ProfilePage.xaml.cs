using AppTodoPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppTodoPro;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel viewModel;

    public ProfilePage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<ProfileViewModel>();
        BindingContext = viewModel;
        viewModel.SignedOut += OnSignedOut;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }

    private async void OnSignedOut(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
