using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersCars.ViewModels;

public class VMPlayersCarsLayer(VMPlayerList players) : VMMapLayerNew
{
    public VMPlayerList Players { get; } = players;
}