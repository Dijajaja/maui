using AppTodoPro.Services;
using AppTodoPro.ViewModels;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace AppTodoPro;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		Batteries_V2.Init();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<CategoryService>();
		builder.Services.AddSingleton<TodoRepository>();
		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<EditTaskViewModel>();
		builder.Services.AddTransient<StatsViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
