using AppTodoPro.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppTodoPro;

public partial class AppShell : Shell
{
    private readonly AuthService authService;
    private bool initialNavigationDone;

    public AppShell()
    {
        InitializeComponent();
        authService = App.Services.GetRequiredService<AuthService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (initialNavigationDone)
        {
            return;
        }

        initialNavigationDone = true;
        var target = authService.CurrentUserId is null ? "//LoginPage" : "//MainTabs/MainPage";
        await GoToAsync(target);
    }
}
