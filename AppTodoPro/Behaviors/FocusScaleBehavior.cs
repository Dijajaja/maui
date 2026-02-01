using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace AppTodoPro.Behaviors;

public class FocusScaleBehavior : Behavior<VisualElement>
{
    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Focused += OnFocused;
        bindable.Unfocused += OnUnfocused;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.Focused -= OnFocused;
        bindable.Unfocused -= OnUnfocused;
        base.OnDetachingFrom(bindable);
    }

    private async void OnFocused(object? sender, FocusEventArgs e)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android
            || DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return;
        }

        if (sender is not VisualElement element)
        {
            return;
        }

        element.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(GetGlowColor()),
            Radius = 14,
            Offset = new Point(0, 3),
            Opacity = 0.9f
        };

        await Task.WhenAll(
            element.ScaleToAsync(1.02, 120, Easing.CubicOut),
            element.FadeToAsync(0.98, 120, Easing.CubicOut));
    }

    private async void OnUnfocused(object? sender, FocusEventArgs e)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android
            || DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return;
        }

        if (sender is not VisualElement element)
        {
            return;
        }

        await Task.WhenAll(
            element.ScaleToAsync(1.0, 120, Easing.CubicOut),
            element.FadeToAsync(1.0, 120, Easing.CubicOut));

        element.Shadow = null;
    }

    private static Color GetGlowColor()
    {
        if (Application.Current?.Resources.TryGetValue("Primary", out var value) == true
            && value is Color color)
        {
            return color.WithAlpha(0.35f);
        }

        return Colors.White.WithAlpha(0.25f);
    }
}
