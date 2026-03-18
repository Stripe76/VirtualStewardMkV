using Avalonia;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.EditingTools;

namespace VirtualSteward.Features.PlayersCars.EditingTools;

public class PlayerCarEdit( VMPlayer player,VMTimeline timeline ) : EditingTool
{
    public override bool LeftMouseDown( Point screenPos,Point trackPos )
    {
        return true;
    }
    public override bool LeftMouseMove( Point screenPos,Point trackPos )
    {
        timeline.CurrentFrame = player.Datasource.GetNearestFrame( trackPos,timeline.CurrentFrame,500,500 );
        
        return true;
    }
    public override bool LeftMouseUp( Point screenPos,Point trackPos )
    {
        return true;
    }
}