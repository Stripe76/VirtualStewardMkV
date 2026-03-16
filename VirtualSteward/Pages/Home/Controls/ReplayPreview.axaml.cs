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

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( e.Properties.IsLeftButtonPressed )
        {
            if( DataContext is not null and VMReplayPreview replayPreview && replayPreview.Commands.Count > 0 )
                replayPreview.Commands[0].RoutedCommand?.Execute( replayPreview.Commands[0].CommandParameter );
        }
    }
}