using Avalonia.Controls.Templates;

using VirtualSteward.Classes;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Timelines.Classes;

namespace VirtualSteward.Features.Timelines;

public class ReplayTimelines : StateFeature
{
    public VMTimeline ReplayTimeline => Timelines[0];
    public VMTimelineList Timelines { get; } = [];
    
    public ReplayTimelines(State state,DataTemplates templates,VMPlayerList players) : base(state,templates)
    {
        Timelines.Add(new VMTimeline("Replay timeline",players));
    }

    public override void AddDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<VMTimeline>( (_, _) => new Controls.Timeline()) );
    }

    public override void OnReplayChanged(VMReplay replay)
    {
        ReplayTimeline.ScrubA = 0;
        ReplayTimeline.ScrubB = ReplayTimeline.End;

        if( ReplayTimeline.Players.Count > 0 )
            ReplayTimeline.Players[0].IsActive = true;
    }
}
