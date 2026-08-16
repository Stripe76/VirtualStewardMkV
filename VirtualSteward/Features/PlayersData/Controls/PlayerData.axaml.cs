using Avalonia.Controls;
using Avalonia.Input;
using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersData.Controls;

public partial class PlayerData : UserControl
{
    public PlayerData( )
    {
        InitializeComponent( );
    }

    private void Player_PointerPressed( object sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and VMPlayer player )
        {
            var point = e.GetCurrentPoint( sender as Control );
            if( point.Properties.IsLeftButtonPressed )
                player.IsActive = !player.IsActive;
            e.Handled = true;
        }
    }
}