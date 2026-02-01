using Microsoft.Maui.ApplicationModel;

namespace AppTodoPro.Services;

public static class ToastService
{
    public static Task ShowAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
#if ANDROID
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, Android.Widget.ToastLength.Short)?.Show();
            return Task.CompletedTask;
#else
            if (Application.Current?.MainPage is Page page)
            {
                return page.DisplayAlert("Info", message, "OK");
            }

            return Task.CompletedTask;
#endif
        });
    }
}
