using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace VirtualSteward.Features.PlayersMessage.Pages;

public partial class PlayersMessage : UserControl
{
    public PlayersMessage( )
    {
        InitializeComponent( );
    }
    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and Features.PlayersMessage.PlayersMessage message )
            message.Options.ShowCalculatedLaptimes.Value = false;
    }
}