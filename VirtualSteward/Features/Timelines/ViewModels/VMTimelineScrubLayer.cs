using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Timelines.ViewModels;

public class VMTimelineScrubLayer( VMTimelineScrubList scrubs ) : VMMapLayer
{
    public VMTimelineScrubList Items => scrubs;
}