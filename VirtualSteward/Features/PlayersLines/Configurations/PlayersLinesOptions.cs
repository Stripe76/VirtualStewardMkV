using Framework.UI.Values;
using Framework.UI.Configurations;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersLines.Configurations;

public class PlayersLinesOptions( PlayersLines playersLines ) : Configuration( "PLAYERS_LINES",null,115 )
{
    public BaseBool LinesVisible = new BaseBool( "LINES_VISIBLE","Players lines" )
    {
        ValueChanged = ( value ) => playersLines.UpdateVisibility( ),
        MinWidth = 90
    };
}