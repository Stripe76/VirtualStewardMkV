using Avalonia;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Checkpoints.ViewModels;

public class VMCheckpointsLayer(VMCheckpointList checkpoints)  : VMMapLayer
{
    public VMCheckpointList Items => checkpoints;
}