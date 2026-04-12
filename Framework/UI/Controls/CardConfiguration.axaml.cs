using Avalonia.Input;
using Avalonia.Controls;

namespace Framework.UI.Controls;

public partial class CardConfiguration : UserControl
{
    public CardConfiguration( )
    {
        InitializeComponent( );
    }

    private void InputElement_OnPointerReleased( object? sender,PointerReleasedEventArgs e )
    {
        if( DataContext is not null and Configurations.Configuration configuration )
            configuration.IsExpanded = !configuration.IsExpanded;
    }
}