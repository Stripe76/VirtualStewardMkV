using Framework.UI.Values;
using Framework.UI.Configurations;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersLines.Configurations;

public class PlayersLinesOptions( VMMapLinesLayer linesLayer ) : Configuration( "PLAYERS_LINES",null,115 )
{
    public BaseBool LinesVisible = new BaseBool( "LINES_VISIBLE","Players lines" )
    {
        ValueChanged = ( value ) => linesLayer.IsVisible = value,
        MinWidth = 90
    };
}