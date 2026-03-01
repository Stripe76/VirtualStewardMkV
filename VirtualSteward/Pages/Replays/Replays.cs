using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.ViewModels;

using VirtualSteward.Classes;
using VirtualSteward.Features.CurrentReplay;
using VirtualSteward.Features.PlayersCars;
using VirtualSteward.Features.PlayersLines;
using VirtualSteward.Features.PlayersList;
using VirtualSteward.Features.ReplayLoading;
using VirtualSteward.Features.Tracklines;
using VirtualSteward.Features.TrackMap;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Realtime;
using VirtualSteward.Features.Timelines;

namespace VirtualSteward.Pages.Replays;

public class Replays : StateFeature
{
    public string Icon { get; } = "\xf1ec";

    public Toolbar Toolbar { get; }
    public TrackMap TrackMap { get; }
    public ReplayTimelines Timelines { get; }
    public ReplayLoading ReplayLoading { get; }

    public UIBaseList Headers { get; } = [];
    public UIBaseList Footers { get; } = [];

    public FeatureCommandList Commands
    {
        get => ReplayLoading.Commands;
    }
    public VMPlayerList Players
    {
        get => _state.Players;
    }
    
    public Replays(State state,DataTemplates templates,string title,Window window,FilesManager filesManager,MessageManager messageManager) : base(state,templates,title)
    {
        Toolbar = new Toolbar();
        TrackMap = new TrackMap( templates );
        Timelines = new ReplayTimelines( state,templates,Players );
        
        _ = new CurrentReplay( state,window );
        _ = new Tracklines( state,templates,TrackMap.Map,filesManager ).AddFooter(Footers);
        _ = new PlayersList( templates,TrackMap,state.Players );
        _ = new PlayersCars( templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players );
        _ = new PlayersLines( templates,TrackMap.Map,state.Players );
        _ = new Realtime( state,templates,Timelines.ReplayTimeline ).AddPage( Headers );

        ReplayLoading = new ReplayLoading( state,templates,filesManager,messageManager );
        ReplayLoading.AddCommands(Toolbar);
    }
    
    public override void AddDataTemplates(DataTemplates templates)
    {
      templates.Add( new FuncDataTemplate<Replays>( (_,_) => new Pages.Replays( ) ) );
    }
}