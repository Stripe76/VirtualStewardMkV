using System.Threading.Tasks;
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
using VirtualSteward.Features.ReplayExport;
using VirtualSteward.Features.ResetReplay;
using VirtualSteward.Features.Timelines;

namespace VirtualSteward.Pages.Replays;

public class Replays : StateFeature
{
    private readonly ResetReplay _replayReset;
    private readonly ReplayLoading _replayLoading;

    public string Icon { get; } = "\xf1ec";

    public Toolbar LeftToolbar { get; }
    public Toolbar RightToolbar { get; }

    public TrackMap TrackMap { get; }
    public ReplayTimelines Timelines { get; }

    public UIBaseList Headers { get; } = [];
    public UIItemList Panels { get; } = [];
    public UIBaseList Footers { get; } = [];

    public VMPlayerList Players => _state.Players;
    
    public Replays(State state,DataTemplates templates,string title,Window window,FilesManager filesManager,MessageManager messageManager) : base(state,templates,title)
    {
        LeftToolbar = new Toolbar();
        RightToolbar = new Toolbar();
        
        TrackMap = new TrackMap( templates );
        Timelines = new ReplayTimelines( state,templates,state.Players );
        
        _ = new CurrentReplay( state,window );
        _replayReset = (ResetReplay)new ResetReplay( state ).AddCommands( RightToolbar );
        _ = new Tracklines( state,templates,TrackMap.Map,filesManager ).AddFooter(Footers);
        _ = new PlayersList( templates );
        _ = new PlayersCars( templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players );
        _ = new PlayersLines( templates,TrackMap.Map,state.Players );

        _replayLoading = (ReplayLoading)new ReplayLoading( state,templates,filesManager,messageManager ).AddCommands( LeftToolbar );

        Panels.Add( new Realtime( state,templates,Timelines.ReplayTimeline ),false,true );
        Panels.Add( new ReplayExport( state,templates,state.Players,Timelines.Timelines,filesManager,messageManager ).AddCommands( LeftToolbar ) );

        Panels.FirstAlwaysActive = true;
    }

    public void ResetReplay( )
    {
        _replayReset.ReplayReset(  );
    }
    public async Task LoadReplay( string filename,bool reset = true,bool makeActive = true )
    {
        if( reset ) ResetReplay( );
        if( makeActive ) IsActive = true;
        //if( makeActive ) IsActive = true;

        await _replayLoading.LoadReplay( filename );
    }
    
    public override Feature AddDataTemplates(DataTemplates templates)
    {
      templates.Add( new FuncDataTemplate<Replays>( (_,_) => new Pages.Replays( ) ) );

      return this;
    }
}