using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.Checkpoints.EditingTools;
using VirtualSteward.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.Features.Checkpoints.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Features.Checkpoints;

public partial class Checkpoints : StateFeature
{
    private readonly VMMap _map;
    private readonly VMCheckpointList _checkpoints = [];
    private readonly VMCheckpointsLayer _checkpointsLayer;

    private readonly CheckpointEdit _editingTool;
    
    private readonly FilesManager _filesManager;

    private bool _editingMode = false,_showCheckpoints = false;

    private VMTracklineFile? _tracklineFile;

    public bool EditingMode
    {
        get => _editingMode;
        set
        {
            if( SetProperty( ref _editingMode,value ) )
                _map.EditingTool = _editingMode ? _editingTool : null;
            IsActive = _editingMode;
        }
    }
    public bool ShowCheckpoints
    {
        get => _showCheckpoints;
        set
        {
            if( SetProperty( ref _showCheckpoints,value ) )
                _checkpointsLayer.IsVisible = _showCheckpoints;
        }
    }

    public FeatureCommandList Commands { get; } = [];
    public VMCheckpointList CheckpointList => _checkpoints; 

    public Checkpoints( State state,DataTemplates templates,FilesManager filesManager,VMMap map ) : base( state,templates,map,null )
    {
        _map = map;
        _filesManager = filesManager;
        _editingTool = new CheckpointEdit( _checkpoints,CreateCheckpoint );
        _checkpoints.CollectionChanged += Checkpoints_CollectionChanged; 
        
        Commands.Add( new FeatureCommand(  )
        {
            IsDefault = true,
            Text = "Save",
            RoutedCommand = SaveCheckpointsCommand 
        } );
        Commands.Add( new FeatureCommand(  )
        {
            Text = "Revert",
            RoutedCommand = RevertCheckpointsCommand 
        } );
        map.AddLayer( _checkpointsLayer = new VMCheckpointsLayer( _checkpoints ) { IsVisible = false } );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<Checkpoints>( ( _,_ ) => new Pages.Checkpoints( ) ) );

        templates.Add( new FuncDataTemplate<VMCheckpoint>( ( _,_ ) => new Controls.Checkpoint( ) ) );
        templates.Add( new FuncDataTemplate<VMCheckpointsLayer>( ( _,_ ) => new TrackMap.Controls.MapItems( ) ) );
        
        return this;
    }

    public override void OnMapChange( VMMap map )
    {
        UpdateCheckpoints( map );
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        _checkpoints.Clear(  );
    }
    public override void OnTracklinesLoaded( VMTrackInfo trackInfo,VMTracklineFile? tracklineFile )
    {
        if( tracklineFile is { Lines.Count: > 0 } )
            _editingTool.Trackline = tracklineFile.Lines[0];
        
        LoadTrackCheckpoints( _checkpoints,trackInfo,_tracklineFile = tracklineFile,_filesManager.VSCheckpointsFolder );
    }

    private void SortCheckpoints( )
    {
        var sorted = _checkpoints.ToList( ); sorted.Sort( );
            
        int i = 1;
        foreach( var cp in sorted )
            cp.Title = i++.ToString( );
    }
    
    private VMCheckpoint CreateCheckpoint( uint frame )
    {
        VMCheckpoint cp = new VMCheckpoint( frame,0 )
        {
            PointerPressed = CheckpointSelectedCommand,
        };
        cp.PropertyChanged += Checkpoint_PropertyChanged;
        cp.UpdateFrame(  );

        return cp;
    }

    [RelayCommand]
    private void CheckpointSelected( VMCheckpoint checkpoint )
    {
        _editingTool.Checkpoint = checkpoint;
    }
    [RelayCommand]
    private void SaveCheckpoints( )
    {
        try
        {
            string filename = _filesManager.GetCheckpointsFileName( _state.Track );

            List<CheckpointSave> arSaves = [];
            foreach( var c in _checkpoints )
                arSaves.Add( new CheckpointSave( c ) );

            using( Stream streamFile = new FileStream( filename,FileMode.Create ) )
            {
                XmlSerializer serializer = new XmlSerializer( typeof(List<CheckpointSave>) );
                using( XmlWriter writer = new XmlTextWriter( streamFile,Encoding.Unicode ) )
                {
                    serializer.Serialize( writer,arSaves );
                }
            }
            EditingMode = false;
        }
        catch( Exception ex )
        {
            //logger?.Error( "Error in LoadCheckpointsList: {message}",ex.Message );
        }
    }
    [RelayCommand]
    private void RevertCheckpoints( )
    {
        LoadTrackCheckpoints( _checkpoints,_state.Track,_tracklineFile,_filesManager.VSCheckpointsFolder );

        EditingMode = false;
    }

    private void UpdateCheckpoints( VMMap map )
    {
        foreach( var checkpoint in _checkpoints )
        {
            checkpoint.MapItem.Position = map.TrackToCanvas( checkpoint.Position.X,checkpoint.Position.Y );
            //checkpoint.MapItem.Scale = _map.Zoom;
            //checkpoint.MapItem.Rotation = Mathematics.Degrees( pos.Rotation.X );
        }
    }
    private void LoadTrackCheckpoints( VMCheckpointList checkpoints,VMTrackInfo trackInfo,VMTracklineFile? tracklineFile,string checkpointsFolder )
    {
        try
        {
            using IsWorking loading = new( IsWorking.Tasks.TrackCheckpointsLoading );

            string file = _filesManager.GetCheckpointsFileName( trackInfo );

            checkpoints.Clear(  );
            
            if( File.Exists( file ) )
            {
                using( Stream streamFile = new FileStream( file,FileMode.Open ) )
                {
                    XmlSerializer serializer = new XmlSerializer(typeof( List<CheckpointSave> ) );
                    using( XmlReader reader = XmlReader.Create( streamFile ) )
                    {
                        List<CheckpointSave>? loaded = (List<CheckpointSave>?)serializer.Deserialize( reader );
                        if( loaded != null )
                        {
                            foreach( var checkpoint in loaded )
                            {
                                //s.Name = n++.ToString( );
                                
                                Point position = new Point( );
                                if( tracklineFile is { FileName: "fast_lane.ai" } )
                                {
                                    if( tracklineFile.Lines is { Count: > 0 } && tracklineFile.Lines[0].Data.Count > checkpoint.Frame )
                                    {
                                        var data = tracklineFile.Lines[0][(int)checkpoint.Frame];

                                        position = new Point( data.Position.X,data.Position.Y );
                                    }
                                }
                                checkpoints.Add( CreateCheckpoint( checkpoint.Frame ) );
                                
                                SortCheckpoints(  );
                            }
                        }
                    }
                }
            }
        }
        catch( TaskAlreadyRunning tx )
        {
            //logger?.Error( "Task already running: {message}",tx.Task );
        }
        catch( Exception ex )
        {
            //logger?.Error( "Error in LoadCheckpointsList: {message}",ex.Message );
        }
    }

    private void Checkpoint_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( sender is not null and VMCheckpoint checkpoint && e.PropertyName is nameof( VMCheckpoint.Frame ) )
        {
            SortCheckpoints(  );
 
            if( _tracklineFile is { Lines: not null } )
            {
                var data = _tracklineFile.Lines[0][(int)checkpoint.Frame];
                checkpoint.Position = new Point( data.Position.X,data.Position.Y );

                checkpoint.MapItem.Position = _map.TrackToCanvas( checkpoint.Position.X,checkpoint.Position.Y );
            }
        }
    }
    private void Checkpoints_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
    {
        if( e is { Action: NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset } )
            SortCheckpoints(  );
    }
}