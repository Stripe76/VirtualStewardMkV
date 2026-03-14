using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using VirtualSteward.Pages.Home.ViewModels;

namespace VirtualSteward.Pages.Home.Controls;

public partial class ReplayPreviewList : UserControl
{
    public ReplayPreviewList( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerReleased( object? sender,PointerReleasedEventArgs e )
    {
        if( e.InitialPressMouseButton == MouseButton.Left && DataContext is not null and VMReplayPreviewList list )
            list.IsExpanded = !list.IsExpanded;
    }
}