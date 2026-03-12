using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Framework.UI;
using VirtualSteward.Features.CarSelection.ViewModels;

namespace VirtualSteward.Features.CarSelection.Controls;

public partial class CarsListItem : UserControl
{
    public CarsListItem( )
    {
        InitializeComponent( );
    }

    private void Select_MouseUp( object? sender,PointerReleasedEventArgs e )
    {
        if( DataContext is not null and UIItem item )
            item.IsActive = true;
    }

    private void Delete_MouseUp( object? sender,PointerReleasedEventArgs e )
    {
        if( DataContext is not null and UIItem item )
        {
            item.IsSelected = false;
            item.IsActive = false;
        }
    }
}