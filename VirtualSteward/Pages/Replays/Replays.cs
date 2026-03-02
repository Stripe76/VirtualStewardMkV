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
using VirtualSteward.Features.ResetReplay;
using VirtualSteward.Features.Timelines;

namespace VirtualSteward.Pages.Replays;

public class Replays : StateFeature
{
    public string Icon { get; } = "\xf1ec";

    public Toolbar LeftToolbar { get; }
    public Toolbar RightToolbar { get; }

    public TrackMap TrackMap { get; }
    public ReplayTimelines Timelines { get; }

    public UIBaseList Headers { get; } = [];
    public UIBaseList Footers { get; } = [];

    public VMPlayerList Players
    {
        get => _state.Players;
    }
    
    public Replays(State state,DataTemplates templates,string title,Window window,FilesManager filesManager,MessageManager messageManager) : base(state,templates,title)
    {
        LeftToolbar = new Toolbar();
        RightToolbar = new Toolbar();
        
        TrackMap = new TrackMap( templates );
        Timelines = new ReplayTimelines( state,templates,Players );
        
        _ = new CurrentReplay( state,window );
        _ = new ResetReplay( state ).AddCommands( RightToolbar );
        _ = new Tracklines( state,templates,TrackMap.Map,filesManager ).AddFooter(Footers);
        _ = new PlayersList( templates );
        _ = new PlayersCars( templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players );
        _ = new PlayersLines( templates,TrackMap.Map,state.Players );
        _ = new Realtime( state,templates,Timelines.ReplayTimeline ).AddPage( Headers );
        _ = new ReplayLoading( state,templates,filesManager,messageManager ).AddCommands( LeftToolbar );
    }
    
    public override Feature AddDataTemplates(DataTemplates templates)
    {
      templates.Add( new FuncDataTemplate<Replays>( (_,_) => new Pages.Replays( ) ) );

      return this;
    }
}