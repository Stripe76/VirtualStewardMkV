using Framework.UI.Configurations;
using Framework.UI.Values;
using VirtualSteward.Features.PlayersLabels.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersLabels.Configurations;

public class PlayersLabelsOptions( VMMapLabelStyle labelStyle, PlayersLabels playersLabels ) : Configuration( "PLAYERS_LABELS" )
{
    public BaseBool LabelsVisible = new BaseBool( "LABELS_VISIBLE","Players name" )
    {
        ValueChanged = ( value ) => playersLabels.UpdateVisibility( )
    };
}