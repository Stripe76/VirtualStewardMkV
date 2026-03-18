using Avalonia.Controls.Templates;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.Timelines;

public class ReplayTimelines : StateFeature
{
    public VMTimeline ReplayTimeline => Timelines[0];
    public VMTimelineList Timelines { get; } = new VMTimelineList( ) { FirstAlwaysActive = true };
    
    public ReplayTimelines(State state,DataTemplates templates,VMPlayerList players) : base(state,templates)
    {
        Timelines.Add( new VMTimeline( "Replay",players ) );
    }

    public override Feature AddDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<VMTimeline>( (_, _) => new Controls.Timeline()) );

        return this;
    }

    public override void OnReplayChanged(VMReplay replay)
    {
        ReplayTimeline.ScrubA = 0;
        ReplayTimeline.ScrubB = ReplayTimeline.End;

        if( ReplayTimeline.Players.Count > 0 )
        {
            ReplayTimeline.Players[0].IsActive = true;
            ReplayTimeline.Players[0].IsActive = false;
        }
    }
}
