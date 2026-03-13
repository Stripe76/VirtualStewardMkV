using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace VirtualSteward.Pages.Home.Pages;

public partial class Home : UserControl
{
    public Home( )
    {
        InitializeComponent( );
    }

    private void ScrollViewer_OnPointerWheelChanged( object? sender,PointerWheelEventArgs e )
    {
        if( sender is not null and ScrollViewer scrollViewer )
        {
            if( e.Delta.Y > 0 )
            {
                scrollViewer.PageLeft( );
            }
            else
            {
                scrollViewer.PageRight( );
            }
        }
    }
}