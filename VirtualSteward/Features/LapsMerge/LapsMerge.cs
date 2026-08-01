using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Datasources;
using VirtualSteward.Features.LapsMerge.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.LapsMerge;

public partial class LapsMerge : StateFeature
{
    private readonly FilesManager _filesManager;

    private readonly VMTimeline _timeline;
    private readonly VMTimelineList _timelines;
    private readonly VMPlayerList _mergedPlayers = new VMPlayerList( true,false );
    private readonly Checkpoints.Checkpoints _checkpoints;

    private bool _syncWithPlayers = true;

    [ObservableProperty] private bool _showInstructions = true;
    [ObservableProperty] private bool _enableCheckpoints = false;

    public Checkpoints.Checkpoints Chekpoints => _checkpoints;

    public bool SyncWithPlayers
    {
        get => _syncWithPlayers;
        set
        {
            if( SetProperty( ref _syncWithPlayers,value ) && _syncWithPlayers )
            {
                SyncSelectedPlayers( );
            }
        }
    }

    public VMPlayerList MergedPlayers => _mergedPlayers;

    public LapsMerge( State state,DataTemplates templates,FilesManager filesManager,VMTimelineList timelines,TrackMap.TrackMap map,Checkpoints.Checkpoints checkpoints ) :
        base( state,templates )
    {
        _filesManager = filesManager;
        _timelines = timelines;
        _timeline = new VMTimeline( "Merged",_mergedPlayers ); // { ShowTitle = true };
        _checkpoints = checkpoints;
        _checkpoints.CheckpointList.CollectionChanged += Checkpoints_CollectionChanged;

        state.Players.SelectedItems.CollectionChanged += SelectedPlayers_CollectionChanged;

        _ = new PlayersLines.PlayersLines( state,null,map.Map,_timeline,_mergedPlayers );
        _ = new PlayersCars.PlayersCars( state,null,map.Map,_timeline,_mergedPlayers );

        PropertyChanged += LapsMerge_PropertyChanged;

        EnableCheckpoints = false;
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<LapsMerge>( ( _,_ ) => new Pages.LapsMerge( ) ) );
        templates.Add( new FuncDataTemplate<MergePlayerLaps>( ( _,_ ) => new Controls.MergePlayerLaps( ) ) );

        return this;
    }

    public override Feature AddCommands( UIItemList commands )
    {
        commands.Add( new ToggleCommand( )
        {
            Icon = "\xf208",
            Tooltip = "Laps merge",
            Object = this,
            Property = "IsVisible"
        } );
        return this;
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        IsVisible = false;
        EnableCheckpoints = false;
        Chekpoints.EditingMode = false;
    }

    #region Merge manager
    private void SyncSelectedPlayers( )
    {
        uint frame = _timeline.CurrentFrame;
        
        _mergedPlayers.Clear( );

        foreach( var player in _state.Players.SelectedItems )
        {
            player.Laps.SelectedItems.CollectionChanged -= SelectedPlayers_CollectionChanged;
            player.Laps.SelectedItems.CollectionChanged += SelectedPlayers_CollectionChanged;
        }
        AddSelectedPlayersLaps( _state.Players );

        _timeline.CurrentFrame = frame;
    }

    private void AddPlayerLaps( VMPlayer player,bool updateTimeline )
    {
        foreach( var lap in player.Laps.SelectedItems )
        {
            VMPlayer? newPlayer = CreateMergedLapCar( player,lap,player.PlayerID );
            if( newPlayer != null )
                _mergedPlayers.Add( newPlayer );
        }
    }
    private void AddSelectedPlayersLaps( VMPlayerList players,bool updateTimeline = true )
    {
        foreach( var player in players.SelectedItems )
        {
            AddPlayerLaps( player,false );
        }
        if( EnableCheckpoints )
            AddCheckpoints( );
        
        if( updateTimeline )
            UpdateTimeline( );
        IsVisible = true;
    }
    private void AddCheckpoints( )
    {
        var sorted = Chekpoints.CheckpointList.ToList(  ); sorted.Sort( );

        foreach( var cp in sorted )
        {
            List<uint> checkFrames = [];
            foreach( var player in _mergedPlayers )
            {
                uint frame = 0;
                if( cp.Frame <= 200 )
                    frame = player.Datasource.GetNearestFrame( cp.Position,0,0,200 );
                else
                    frame = player.Datasource.GetNearestFrame( cp.Position,0,-1,-1 );

                checkFrames.Add( frame );
            }
            if( checkFrames.Count > 0 && checkFrames.Count == _mergedPlayers.Count )
            {
                uint maxFrame = checkFrames.Max( );

                int n = 0;
                foreach( var player in _mergedPlayers )
                {
                    if( player.Datasource is not null and SegmentedDatasource datasource )
                    {
                        uint frame = checkFrames[n++];
                        int diff = (int)(maxFrame - frame);

                        if( diff > 0 )
                        {
                            InsertOffset( datasource,frame,diff );
                        }
                    }
                }
            }
        }
        return;

        void InsertOffset( SegmentedDatasource datasource,uint frame,int width )
        {
            SegmentList segments = datasource.Segments; 
            int keyFrame = segments.GetSegmentIndexFromFrame( frame );
            if( keyFrame >= 0 )
            {
                Segment oldSegment = segments[keyFrame];
                segments.Remove( oldSegment );

                uint segmentStart = segments.GetSegmentVirtualStart( frame );
                frame -= segmentStart;

                Segment beforeSegment = new( oldSegment.Start,(int)(frame) );
                Segment afterSegment = new( beforeSegment.End,(int)(oldSegment.End-beforeSegment.End) );
                Segment offsetSegment = new( beforeSegment.End,-width );

                segments.Insert( keyFrame,afterSegment );
                segments.Insert( keyFrame,offsetSegment );
                segments.Insert( keyFrame,beforeSegment );
            }
        }
    }

    private VMPlayer? CreateMergedLapCar( VMPlayer copyPlayer,VMPlayerLap lap,int playerID,uint paddingFrames = 60 )
    {
        IImmutableSolidColorBrush carColor = VMMapLineStyle.LineColors[playerID % VMMapLineStyle.LineColors.Count];

        SegmentedDatasource datasource = new SegmentedDatasource( copyPlayer.Datasource );
        VMPlayer newPlayer = new(
            playerID,
            copyPlayer,
            datasource,
            _state.GetPlayerLabelStyle(  ),
            _state.GetPlayerLineStyle( playerID ),
            _state.GetPlayerCarImage( playerID,copyPlayer.PlayerInfo.CarInfo.CarID,copyPlayer.PlayerInfo.CarSkinInfo.SkinID ),
            VMPlayer.ShowCommand.Edit /*| VMPlayer.ShowCommand.Delete*/
        );
        newPlayer.PlayerInfo.PlayerName = copyPlayer.PlayerInfo.PlayerName + " - Lap " + lap.LapName;
        
        uint start = lap.StartFrame;
        uint end = lap.EndFrame;

        if( end - start > 20 )
        {
            if( (int)start - paddingFrames < 0 )
                paddingFrames += start - paddingFrames;

            datasource.Segments.Add( new Segment( start - paddingFrames,(int)(end - start + paddingFrames * 2) ) );

            VMPlayerLap newLap = new( lap.LapNumber,paddingFrames,paddingFrames + (end - start),newPlayer.LineStyle )
            {
                LapTime = lap.LapTime,
            };
            newPlayer.Laps.Clear(  );
            newPlayer.Laps.Add( newLap );

            return newPlayer;
        }
        return null;
    }
    #endregion

    #region Timeline manager
    private void UpdateTimeline( )
    {
        if( IsVisible && MergedPlayers.Count > 0 )
        {
            if( _timelines.AddIfNotContains( _timeline ) )
            {
                //_timeline.ReplayFrequency = _state.Replay.ReplayFrequency;

                _timeline.ScrubA = _timeline.Start = 0;
                _timeline.ScrubB = _timeline.End = _timeline.TotalLength;

                _timeline.CurrentFrame = 0;
            }
            else
            {
                _timeline.End = _timeline.TotalLength;
                if( _timeline.ScrubB == 0 )
                    _timeline.ScrubB = _timeline.End;
                
                _timeline.CurrentFrame = _timeline.CurrentFrame;
            }
        }
        else
        {
            _timeline.IsActive = false;
            _timelines.Remove( _timeline );
        }
        bool show = _timelines.Count > 1;
        foreach( var timeline in _timelines )
            timeline.ShowName = show;
        ShowInstructions = MergedPlayers.Count <= 0;
    }
    #endregion

    [RelayCommand]
    private void AddPlayerLaps( VMPlayer player )
    {
        AddPlayerLaps( player,true );
    }
    [RelayCommand]
    private void SwitchCheckpointsEditing( )
    {
        Chekpoints.EditingMode = !Chekpoints.EditingMode;
    }
    [RelayCommand]
    private void SwitchEnableCheckpoints( )
    {
        EnableCheckpoints = _checkpoints.ShowCheckpoints = !EnableCheckpoints;

        if( !EnableCheckpoints )
            _checkpoints.EditingMode = false;
        else if ( _checkpoints.CheckpointList.Count <= 0 )
            _checkpoints.EditingMode = true;
        
        SyncSelectedPlayers( );
    }
    [RelayCommand]
    private void RemoveNoLapsPlayers( )
    {
        VMPlayerList players = _state.Players;
        VMPlayerList toRemove = [];

        foreach( var player in players )
        {
            if( player.IsNoLapPlayer )
                toRemove.Add( player );
        }
        foreach( var player in toRemove )
        {
            players.Remove( player );
        }
    }

    private void LapsMerge_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( e.PropertyName is nameof( IsVisible ) )
        {
            /*
            foreach( var player in _state.Players )
            {
                player.Header = IsVisible
                    ? new MergePlayerLaps( new FeatureCommand( )
                    {
                        Icon = "\xf2c2",
                        Tooltip = "Merge selected laps",
                        RoutedCommand = AddPlayerLapsCommand,
                        CommandParameter = player
                    } )
                    : null;
            }
            */
            if( IsVisible && SyncWithPlayers )
                SyncSelectedPlayers( );
            UpdateTimeline(  );
        }
    }
    private void Checkpoints_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
    {
        if( EnableCheckpoints && e is { Action: NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset } )
            SyncSelectedPlayers( );
    }
    private void SelectedPlayers_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
    {
        if( _syncWithPlayers && IsVisible )
        {
            if( e.Action == NotifyCollectionChangedAction.Add  || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset  )
            {
                SyncSelectedPlayers( );
            }
        }
    }
}