using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace AppTodoPro;

public partial class App : Application
{
    private const string ThemePreferenceKey = "theme_preference";
    private const string CrashLogFileName = "app-crash.log";
    public static IServiceProvider Services { get; private set; } = default!;

    public App(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        ApplyStoredTheme();

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
        {
            LogException("AndroidEnvironment.UnhandledExceptionRaiser", args.Exception);
            args.Handled = false;
        };
#endif

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                LogException("AppDomain.UnhandledException", ex);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogException("TaskScheduler.UnobservedTaskException", args.Exception);
            args.SetObserved();
        };

        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            LogException("InitializeComponent", ex);
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        MainThread.BeginInvokeOnMainThread(() => _ = ShowCrashLogIfAnyAsync(window));
        return window;
    }

    private static void LogException(string source, Exception exception)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, CrashLogFileName);
            var message = $"[{DateTime.UtcNow:O}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(path, message);
            Console.WriteLine(message);
#if ANDROID
            Android.Util.Log.Error("AppTodoPro", message);
#endif
        }
        catch
        {
        }
    }

#if ANDROID
    private static void LogJavaThrowable(string source, Java.Lang.Throwable exception)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, CrashLogFileName);
            var message = $"[{DateTime.UtcNow:O}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(path, message);
            Console.WriteLine(message);
            Android.Util.Log.Error("AppTodoPro", message);
        }
        catch
        {
        }
    }
#endif

    private static async Task ShowCrashLogIfAnyAsync(Window window)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, CrashLogFileName);
            if (!File.Exists(path))
            {
                return;
            }

            var text = await File.ReadAllTextAsync(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                File.Delete(path);
                return;
            }

            var excerpt = text.Length > 1600 ? $"{text[..1600]}{Environment.NewLine}...[tronque]" : text;
            await Clipboard.SetTextAsync(text);
            if (window.Page is not null)
            {
                await window.Page.DisplayAlertAsync(
                    "Crash detecte",
                    $"Le log a ete copie dans le presse-papiers.{Environment.NewLine}{Environment.NewLine}{excerpt}",
                    "OK");
            }

            var archived = Path.Combine(FileSystem.AppDataDirectory, "app-crash.last.log");
            File.WriteAllText(archived, text);
            File.Delete(path);
        }
        catch
        {
        }
    }

    private void ApplyStoredTheme()
    {
        var stored = Preferences.Get(ThemePreferenceKey, "system");
        UserAppTheme = stored switch
        {
            "dark" => AppTheme.Dark,
            "light" => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
    }
}