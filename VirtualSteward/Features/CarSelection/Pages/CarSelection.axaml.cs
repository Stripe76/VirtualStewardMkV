using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace VirtualSteward.Features.CarSelection.Pages;

public partial class CarSelection : UserControl
{
    public CarSelection( )
    {
        InitializeComponent( );
    }

    private void Close_MouseUp( object? sender,PointerReleasedEventArgs e )
    {
        throw new System.NotImplementedException( );
    }
}