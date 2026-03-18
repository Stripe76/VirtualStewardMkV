using Avalonia;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.EditingTools;

namespace VirtualSteward.Features.Timelines.EditingTools;

public class TimelineScrubEdit( VMPlayer player,VMTimelineScrub scrub ) : EditingTool
{
    public override bool LeftMouseDown( Point screenPos,Point trackPos )
    {
        return true;
    }
    public override bool LeftMouseMove( Point screenPos,Point trackPos )
    {
        scrub.SetTimelineFrame( player.Datasource.GetNearestFrame( trackPos,scrub.Frame,500,500 ) );
        
        return true;
    }
    public override bool LeftMouseUp( Point screenPos,Point trackPos )
    {
        return true;
    }
}
