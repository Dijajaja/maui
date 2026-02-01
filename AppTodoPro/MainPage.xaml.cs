using AppTodoPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace AppTodoPro;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel viewModel;

    public MainPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<MainPageViewModel>();
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

    private async void OnReorderCompleted(object? sender, EventArgs e)
    {
        await viewModel.PersistOrderAsync();
    }

    private async void OnTaskCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value)
        {
            return;
        }

        if (sender is not CheckBox checkBox)
        {
            return;
        }

        var border = checkBox.Parent?.Parent as Border;
        if (border is null)
        {
            return;
        }

        await border.FadeToAsync(0.4, 180);
        await border.TranslateToAsync(20, 0, 180, Easing.CubicIn);
        await border.FadeToAsync(1, 1);
        border.TranslationX = 0;
    }
}
