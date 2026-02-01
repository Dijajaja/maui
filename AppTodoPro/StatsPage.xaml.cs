using AppTodoPro.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppTodoPro;

public partial class StatsPage : ContentPage
{
    private readonly StatsViewModel viewModel;

    public StatsPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<StatsViewModel>();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}
