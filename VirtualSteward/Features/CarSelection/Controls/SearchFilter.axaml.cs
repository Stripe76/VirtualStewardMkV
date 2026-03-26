using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Framework.UI.Values;

namespace VirtualSteward.Features.CarSelection.Controls;

public partial class SearchFilter : UserControl
{
    public SearchFilter( )
    {
        InitializeComponent( );
    }
    private void Button_OnClick( object? sender,RoutedEventArgs e )
    {
        if( DataContext is not null and BaseValue<string> value )
            value.Value = "";
    }
}