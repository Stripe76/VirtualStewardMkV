using Avalonia.Markup.Xaml;
using Avalonia.Styling;

// ReSharper disable once CheckNamespace
namespace Framework;

/// <summary>
///     The main theme for the application.
/// </summary>
public class FrameworkTheme : Styles
{
    /// <summary>
    ///     Returns a new instance of the <see cref="ShadTheme" /> class.
    /// </summary>
    public FrameworkTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}