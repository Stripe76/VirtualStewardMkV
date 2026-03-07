using Avalonia.Input;
using Avalonia.Controls;

using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersList.Controls;

public partial class Player : UserControl
{
    public Player()
    {
        InitializeComponent();
    }
    
    private void HandleClick(object sender, PointerPressedEventArgs e)
    {
        // Event handling logic goes here
        if (DataContext is not null and VMPlayer player)
        {
            var point = e.GetCurrentPoint(sender as Control);
            if (point.Properties.IsLeftButtonPressed)
                player.IsActive = !player.IsActive;
            else if (point.Properties.IsRightButtonPressed)
                player.IsSelected = !player.IsSelected;
            e.Handled = true;
        }
    }

    private void Visible_PointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and VMPlayer player )
            player.IsVisible = !player.IsVisible;
        e.Handled = true;
    }
}