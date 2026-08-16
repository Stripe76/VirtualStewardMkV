using Avalonia;
using Avalonia.Controls.Templates;
using VirtualSteward.Classes;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersFollow;

public partial class PlayersFollow : StateFeature
{
    private readonly VMMap _map;
    private readonly VMPlayerList _players;

    public PlayersFollow( State state,DataTemplates templates,VMMap map,VMTimeline timeline,VMPlayerList players ) : base( state,templates,null,timeline )
    {
        _map = map;
        _players = players;
        _players.ActiveItemChanged += ( sender,player ) => { OnTimelineChange( timeline,StateFeature.TimelineChangeType.CurrentFrame ); };
    }

    public override void OnTimelineChange( VMTimeline timeline,StateFeature.TimelineChangeType type )
    {
        if( type == TimelineChangeType.CurrentFrame )
        {
            if( _players.ActiveItem is { IsSelected: true } )
            {
                var player = _players.ActiveItem; 
                VMCarPosition? pos = player.Datasource.GetPositionAndRotation( timeline.CurrentFrame );
                if( pos != null )
                {
                    _map.CenterOn = new Point( pos.Position.X,pos.Position.Y );
                }
            }
        }
    }
}
