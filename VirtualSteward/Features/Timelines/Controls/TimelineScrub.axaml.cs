using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.Timelines.Controls;

public partial class TimelineScrub : UserControl
{
    public TimelineScrub( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and VMTimelineScrub scrub && e.Properties.IsLeftButtonPressed )
            scrub.PointerPressed?.Execute( scrub );
    }
}