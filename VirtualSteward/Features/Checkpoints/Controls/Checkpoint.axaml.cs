using Avalonia.Controls;
using Avalonia.Input;
using VirtualSteward.Features.Checkpoints.ViewModels;

namespace VirtualSteward.Features.Checkpoints.Controls;

public partial class Checkpoint : UserControl
{
    public Checkpoint( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and VMCheckpoint checkpoint )
            checkpoint.PointerPressed?.Execute( checkpoint );
    }
}