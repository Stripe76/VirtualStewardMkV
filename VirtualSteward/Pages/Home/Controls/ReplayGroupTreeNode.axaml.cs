using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using VirtualSteward.Pages.Home.ViewModels;

namespace VirtualSteward.Pages.Home.Controls;

public partial class ReplayGroupTreeNode : UserControl
{
    public ReplayGroupTreeNode( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerReleased( object? sender,PointerReleasedEventArgs e )
    {
        if( DataContext is not null and VMReplayGroupTreeNode node )
            node.IsSelected = true;
    }
}