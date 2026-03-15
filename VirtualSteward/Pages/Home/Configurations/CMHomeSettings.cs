using Framework.UI.Configurations;
using Framework.UI.Values;

namespace VirtualSteward.Pages.Home.Configurations;

public class CMHomeSettings( Home home ) : Configuration( "HOME_SETTINGS" )
{
    public readonly BaseBool LatestCollapsed = new BaseBool( nameof( LatestCollapsed ) )
    {
        ValueChanged = (value) => home.LatestReplays.IsExpanded = !value 
    };
    public readonly BaseBool RecentCollapsed = new BaseBool( nameof( RecentCollapsed ) )
    {
        ValueChanged = (value) => home.RecentReplays.IsExpanded = !value 
    };
}