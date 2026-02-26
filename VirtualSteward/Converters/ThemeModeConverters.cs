using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ShadUI;

namespace VirtualSteward.Converters;

public static class ThemeModeConverters
{
    private static readonly Dictionary<ThemeMode, string> Icons = new()
    {
        { ThemeMode.System, "\uF1A5" },
        { ThemeMode.Light, "\uF2FB" },
        { ThemeMode.Dark, "\uF26C" }
    };

    public static readonly IValueConverter ToLucideIcon =
        new FuncValueConverter<ThemeMode, string>(mode => Icons.TryGetValue(mode, out var icon) ? icon : Icons[0]);
}

public static class WindowStateConverters
{
    public static readonly IValueConverter IsFullScreen =
        new FuncValueConverter<WindowState, bool>(state => state == WindowState.FullScreen);

    public static readonly IValueConverter IsNotFullScreen =
        new FuncValueConverter<WindowState, bool>(state => state != WindowState.FullScreen);
}