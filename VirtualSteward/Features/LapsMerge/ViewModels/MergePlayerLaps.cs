using Framework.UI;

namespace VirtualSteward.Features.LapsMerge.ViewModels;

public class MergePlayerLaps( FeatureCommand command ) : UIBase
{
    public FeatureCommand Command { get; } = command;
}