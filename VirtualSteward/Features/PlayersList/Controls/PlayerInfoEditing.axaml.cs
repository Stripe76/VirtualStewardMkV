using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace VirtualSteward.Features.PlayersList.Controls;

public partial class PlayerInfoEditing : UserControl
{
    public PlayerInfoEditing( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        e.Handled = true;
    }
}