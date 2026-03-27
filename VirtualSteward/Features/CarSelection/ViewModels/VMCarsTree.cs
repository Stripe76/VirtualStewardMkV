using System;
using Framework.UI.ViewModels;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarsTree : TreePath<VMCarInfo>
{
    public VMSearchFilter SearchFilter { get; }

    public VMCarsTree( VMCarInfoList carsList ) : base( carsList,["Brand"] )
    {
        ShowCheckbox = true;

        SearchFilter = new VMSearchFilter( )
        {
            MinWidth = 300,
            ValueChanged = (value) => SearchFilterChanged( value ?? "" ) 
        };
    }

    private void SearchFilterChanged( string? searchFilter )
    {
        if( IsValidSearchText( searchFilter ) )
        {
            string[] keys = searchFilter.Split( " " );
            foreach( var info in Items )
            {
                info.IsEnabled = IsMatch( info,keys );
            }
            ExpandAll = true;

            Refresh(  );
        }
        else if( ExpandAll )
        {
            foreach( var info in Items )
                info.IsEnabled = true;
            ExpandAll = false;

            Refresh(  );
        }
    }
    
    private static readonly char[] _numbers = ['0','1','2','3','4','5','6','7','8','9'];
    private static bool IsValidSearchText( string? searchText )
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
                if( info.SearchKeys.IndexOf( key,StringComparison.Ordinal ) < 0 )
                    return false;
            }
            return true;
        }
        return false;
    }
}