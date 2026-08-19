using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.ViewModels;

using VirtualSteward.Classes;
using VirtualSteward.Features.Checkpoints;
using VirtualSteward.Features.CurrentReplay;
using VirtualSteward.Features.FileTemplates;
using VirtualSteward.Features.LapsMerge;
using VirtualSteward.Features.PlayersCars;
using VirtualSteward.Features.PlayersData;
using VirtualSteward.Features.PlayersFollow;
using VirtualSteward.Features.PlayersLabels;
using VirtualSteward.Features.PlayersLines;
using VirtualSteward.Features.PlayersList;
using VirtualSteward.Features.ReplayLoading;
using VirtualSteward.Features.Tracklines;
using VirtualSteward.Features.TrackMap;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.PlayersMessage;
using VirtualSteward.Features.Realtime;
using VirtualSteward.Features.Realtime.ViewModels;
using VirtualSteward.Features.ReplayExport;
using VirtualSteward.Features.ResetReplay;
using VirtualSteward.Features.Timelines;

namespace VirtualSteward.Pages.Replays;

public class Replays : StateFeature
{
    private readonly Realtime _realtime;
    private readonly ResetReplay _replayReset;
    private readonly ReplayLoading _replayLoading;

    public string Icon { get; } = "\xf1ec";

    public Toolbar LeftToolbar { get; }
    public Toolbar RightToolbar { get; }

    public TrackMap TrackMap { get; }
    public ReplayTimelines Timelines { get; }

    public LapsMerge LapsMerge { get; }
    public PlayersData PlayersData { get; }
    public PlayersMessage PlayersMessage { get; }

    public PlayersData PlayersDataMerge { get; }

    public UIBaseList Headers { get; } = [];
    public UIItemList Panels { get; } = new UIItemList( ) { MultiActiveEnabled = true };
    public UIBaseList Footers { get; } = [];

    public VMPlayerList Players => _state.Players;
    
    public VMFrameValidationTimeline FrameValidation => _realtime.FrameValidation;
    
    public Replays( State state,
                    DataTemplates templates,
                    string title,
                    Window window,
                    FilesManager filesManager,
                    MessageManager messageManager ) : base( state,templates,title )
    {
        LeftToolbar = new Toolbar();
        RightToolbar = new Toolbar();
        
        TrackMap = new TrackMap( templates );
        Timelines = new ReplayTimelines( state,templates,state.Players );
        
        _ = new CurrentReplay( state,window );
        _replayReset = (ResetReplay) new ResetReplay( state ).AddCommands( RightToolbar );
        _ = new Tracklines( state,templates,TrackMap.Map,filesManager ).AddFooter( Footers );
        _ = new PlayersList( templates );
        _ = new PlayersLines( state,templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players ).AddFooter( Footers );
        _ = new PlayersCars( state,templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players );
        _ = new PlayersFollow( state,templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players );

        PlayersData = (PlayersData) new PlayersData( state,templates,Timelines.ReplayTimeline,state.Players ).AddFooter( Footers );

        AddLoadingPage( new PlayersLabels( state,templates,TrackMap.Map,Timelines.ReplayTimeline,state.Players ).AddFooter( Footers ) );
        AddLoadingPage( PlayersMessage = new PlayersMessage( state,templates ) );

        _replayLoading = (ReplayLoading) new ReplayLoading( state,templates,filesManager,messageManager ).AddCommands( LeftToolbar );

        Panels.Add( _realtime = new Realtime( state,templates,Timelines.ReplayTimeline ),false,true );
        Panels.Add( new ReplayExport( state,templates,state.Players,Timelines.Timelines,new FileTemplates( templates,filesManager ).TemplateFiles,filesManager,messageManager ).AddCommands( LeftToolbar ) );

        Checkpoints chekpoints = new Checkpoints( state,templates,filesManager,TrackMap.Map );
        Panels.Add( chekpoints );
        
        LapsMerge = (LapsMerge) new LapsMerge( state,templates,filesManager,Timelines.Timelines,TrackMap,chekpoints ) { IsVisible = false }.AddCommands( LeftToolbar );
        PlayersDataMerge = new PlayersData( state,null,LapsMerge.Timeline,LapsMerge.MergedPlayers );
        _ = new PlayersFollow( state,templates,TrackMap.Map,LapsMerge.Timeline,LapsMerge.MergedPlayers );
        
        _ = new TimelinesScrubs( state,templates,state.Players,TrackMap.Map,Timelines.ReplayTimeline );

        Panels.FirstAlwaysActive = true;
    }

    public override Feature AddDataTemplates(DataTemplates templates)
    {
      templates.Add( new FuncDataTemplate<Replays>( (_,_) => new Pages.Replays( ) ) );

      return this;
    }

    public void ResetReplay( )
    {
        _replayReset.ReplayReset(  );
    }
    
    public async Task LoadReplay( string filename,bool reset = true,bool makeActive = true )
    {
        if( reset ) ResetReplay( );
        if( makeActive ) IsActive = true;

        await _replayLoading.LoadReplay( filename );
    }
    public async Task MergeReplay( string filename,bool reset = true,bool makeActive = true )
    {
        if( _state.Replay.IsLoaded )
            await _replayLoading.MergeReplay( filename );
        else
            await _replayLoading.LoadReplay( filename );
    }
}