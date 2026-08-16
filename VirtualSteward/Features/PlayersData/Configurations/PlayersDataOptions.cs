using Framework.UI.Configurations;
using Framework.UI.Values;

namespace VirtualSteward.Features.PlayersData.Configurations;

public class PlayersDataOptions( PlayersData playersData ) : Configuration( "PLAYERS_DATA",null,115 )
{
    public BaseBool DataVisible = new BaseBool( "DATA_VISIBLE","Players data" )
    {
        ValueChanged = ( value ) => playersData.UpdateVisibility(  ),
        MinWidth = 90
    };
}