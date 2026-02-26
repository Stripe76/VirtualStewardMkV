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
    
    private void HandleClick(object sender, PointerPressedEventArgs args)
    {
        // Event handling logic goes here
        if (DataContext is not null and VMPlayer player)
        {
            var point = args.GetCurrentPoint(sender as Control);
            if (point.Properties.IsLeftButtonPressed)
                player.IsActive = !player.IsActive;
            else if (point.Properties.IsRightButtonPressed)
                player.IsSelected = !player.IsSelected;
        }
    }
}