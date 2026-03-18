using Avalonia.Controls;
using Avalonia.Input;
using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersCars.Controls;

public partial class PlayersCars : UserControl
{
    public PlayersCars()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( sender is not null and Image { DataContext: not null and VMPlayer player } )
            player.CarImage.PointerPressed?.Execute( player );
    }
}