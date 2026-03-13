using Avalonia.Controls;
using Avalonia.Input;
using VirtualSteward.Pages.Home.ViewModels;

namespace VirtualSteward.Pages.Home.Controls;

public partial class ReplayPreview : UserControl
{
    public ReplayPreview( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerReleased( object? sender,PointerReleasedEventArgs e )
    {
        if( e.InitialPressMouseButton == MouseButton.Left )
        {
            if( DataContext is not null and VMReplayPreview replayPreview && replayPreview.Commands.Count > 0 )
                replayPreview.Commands[0].RoutedCommand?.Execute( replayPreview.Commands[0].CommandParameter );
        }
    }
}