using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersList.Controls;

public partial class PlayerLaps : UserControl
{
    public PlayerLaps()
    {
        InitializeComponent();
    }

    private void Lap_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not null and Control tb && tb.DataContext is not null and VMPlayerLap lap)
        {
            lap.IsSelected = !lap.IsSelected;

            e.Handled = true;
        }
    }
}