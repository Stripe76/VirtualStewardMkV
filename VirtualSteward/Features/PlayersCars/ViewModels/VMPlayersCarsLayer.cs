using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersCars.ViewModels;

public class VMPlayersCarsLayer(VMPlayerList players) : VMMapLayer
{
    public VMPlayerList Players { get; } = players;
}