using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Framework.UI.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;

namespace VirtualSteward.Features.CarSelection.Controls;

public partial class CarsSelector : UserControl
{
    public object? Cars { get; set; }
    
    public CarsSelector( )
    {
        InitializeComponent( );

        //if( DataContext is ObservableCollectionEx<IMultiListItem> items )
            //Cars = new TreePath( items,["Brand/Model"] );
        //tpvCarsList.ShowCheckbox = true;

        DataContextChanged += OnDataContextChanged;
    }

    private void FilterCars( string searchFilter )
    {
        if( DataContext is not null and VMCarInfoList carsList )
        {
            if( IsValidSearchText( searchFilter ) )
            {
                string[] keys = searchFilter.Split( " " );
                foreach( var info in carsList )
                {
                    info.IsEnabled = IsMatch( info,keys );
                }
                //tpvCarsList.ExpandAll = true;
            }
            else
            {
                foreach( var info in carsList )
                    info.IsEnabled = true;
                //tpvCarsList.ExpandAll = false;
            }
            /*
            tpvCarsList.SetItems( [.. carsList],
            [
                "Brand/Model"
            ] );
            */
        }
    }

    private static readonly char[] _numbers = ['0','1','2','3','4','5','6','7','8','9'];
    private static bool IsValidSearchText( string searchText )
    {
        if( string.IsNullOrEmpty( searchText ) )
            return false;

        string[] keys = searchText.Split( " " );
        foreach( string key in keys )
        {
            if( key.Length >= 3 )
                return true;
            if( key.Length >= 2 && searchText.IndexOfAny( _numbers ) >= 0 )
                return true;
        }
        return false;
    }
    private static bool IsMatch( VMCarInfo info,string[] keys )
    {
        info.SearchKeys ??= info.Brand.ToLower( ) + " " + info.Model.ToLower( );

        if( info.SearchKeys != null )
        {
            foreach( string key in keys )
            {
                if( info.SearchKeys.IndexOf( key ) < 0 )
                    return false;
            }
            return true;
        }
        return false;
    }

    private void OnDataContextChanged( object? sender,EventArgs e )
    {
        //if( DataContext is not null and VMCarInfoList carsList )
        {
            //carsList.CollectionChanged -= CarsListOnCollectionChanged;
            //carsList.CollectionChanged += CarsListOnCollectionChanged;
            if( DataContext is VMCarInfoList items )
            {
                Cars = new TreePath<VMCarInfo>( items,["Brand/Model"] );

                //OnPropertyChanged(  );


                //items.Refresh(  );
            }
            //carsList.Refresh(  );
        }
    }
    private void CarsListOnCollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
    {
        if( e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset )
        {
            if( sender is not null and ObservableCollection<VMCarInfo> carsList )
            {
                /*
                tpvCarsList.SetItems( [.. carsList],
                [
                    "Brand/Model"
                ] );
                */
            }
        }
    }

    private void SearchFilter_TextChanged( object? sender,TextChangedEventArgs e )
    {
        FilterCars( tbSearchFilter.Text );
    }
    private void ResetSearch_Click( object? sender,RoutedEventArgs e )
    {
        tbSearchFilter.Text = "";
    }
}