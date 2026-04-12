using System.Collections.Generic;
using System.ComponentModel;
using ACLibrary.Cars;
using ACLibrary.Tracks;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

using Framework.UI;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Classes;

public partial class State : ObservableObject
{
    private readonly FilesManager _filesManager;

    private readonly SortedList<string,CarInfo> _cars = [];
    private readonly SortedList<string,TrackInfo> _tracks = [];
    private readonly VMMapLabelStyle _playerLabelStyle = new ( ); 

    [ObservableProperty] private string _ACFolder = "";
    [ObservableProperty] private string _replaysFolder = "";

    [ObservableProperty] private VMReplay _replay = new( );
    [ObservableProperty] private VMCarInfo _car = new( "" );
    [ObservableProperty] private VMTrackInfo _track = new( "","" );
    [ObservableProperty] private VMTracklineFile? _tracklineFile;
    
    public VMPlayerList Players { get; } = new( true,false );

    public State( FilesManager filesManager)
    {
        _filesManager = filesManager;
    }

    public VMMapLineStyle GetPlayerLineStyle( int playerID )
    {
        return new VMMapLineStyle( 2,VMMapLineStyle.LineColors[playerID % VMMapLineStyle.LineColors.Count] );
    }
    public VMMapLabelStyle GetPlayerLabelStyle( )
    {
        return _playerLabelStyle;
    }
    public VMMapImage GetPlayerCarImage( int playerID,string carID,string skinID )
    {
        return new VMMapImage( _filesManager.GetCarImage( carID,skinID,VMMapLineStyle.LineColors[playerID % VMMapLineStyle.LineColors.Count] ) );
    }

    public CarInfo GetCarInfo( string carID )
    {
        if( _cars.TryGetValue( carID,out CarInfo? value ) )
            return value;
        
        CarInfo? info = CarInfo.LoadCarInfo( _filesManager.ACCarsFolder,carID );
        if( info != null )
        {
            lock( _cars )
            {
                _cars.TryAdd( carID,info );
            }
            return info;
        }
        return new CarInfo( carID );
    }
    public TrackInfo GetTrackInfo( string trackID,string variantID,bool setCSPSettingsFile )
    {
        string key = trackID + "_" + variantID;
        if( _tracks.TryGetValue( key,out TrackInfo? value ) )
        {
            //if( setCSPSettingsFile && value.CSPSettingsFilePath == null )
//                value.CSPSettingsFilePath = GetCSPSettingsFile( value.TrackID );
            return value;
        }
        TrackInfo? info = TrackInfo.LoadTrackInfo( _filesManager.ACTracksFolder,trackID,variantID );
        if( info != null )
        {
            VMTrackInfo newInfo = new ( info,_filesManager.ACTracksFolder );
            //if( setCSPSettingsFile && newInfo.CSPSettingsFilePath == null )
//                newInfo.CSPSettingsFilePath = GetCSPSettingsFile( newInfo.TrackID );

            lock( _tracks )
            {
                _tracks.TryAdd( key,info );
            }
            return info;
        }
        return new TrackInfo( trackID,variantID );
    }
}

public class StateFeature : Feature
{
    public enum TimelineChangeType
    {
        Scrubs,
        IsActive,
        CurrentFrame,
    }
    
    protected readonly State _state;

    public StateFeature( State state,string headerTitle = "" ) : base( null,headerTitle )
    {
        _state = state;
        _state.PropertyChanged += State_PropertyChanged;
    }
    public StateFeature( State state,DataTemplates? templates,string headerTitle = "" ) : base( templates,headerTitle )
    {
        _state = state;
        _state.PropertyChanged += State_PropertyChanged;
    }
    public StateFeature( State state,DataTemplates? templates,VMMap? map,VMTimeline? timeline = null,string headerTitle = "" ) : base( templates,headerTitle )
    {
        _state = state;
        _state.PropertyChanged += State_PropertyChanged;

        map?.PropertyChanged += Map_PropertyChanged;
        timeline?.PropertyChanged += Timeline_PropertyChanged;
    }

    public virtual void OnACFolderChanged( )
    {
    }
    public virtual void OnReplayFolderChanged( )
    {
    }

    public virtual void OnMapChange( VMMap map )
    {
        
    }
    public virtual void OnTimelineChange( VMTimeline timeline,TimelineChangeType type )
    {
        
    }
    
    public virtual void OnReplayChanged( VMReplay replay )
    {
    }
    public virtual void OnTrackChanged( VMTrackInfo trackInfo )
    {
    }
    public virtual void OnTracklinesLoaded( VMTrackInfo trackInfo,VMTracklineFile? tracklinesFiles )
    {
    }

    private void Map_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( sender is not null and VMMap map && e.PropertyName == nameof( VMMap.Offset ) )
            OnMapChange( map );
    }
    private void State_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
    {
        if( sender is not null and State state && e.PropertyName != null )
        {
            if( e.PropertyName.Equals( nameof( State.Replay ) ) )
                OnReplayChanged( state.Replay );
            else if( e.PropertyName.Equals( nameof( State.Track ) ) )
                OnTrackChanged( state.Track );
            else if( e.PropertyName.Equals( nameof( State.ACFolder ) ) )
                OnACFolderChanged( );
            else if( e.PropertyName.Equals( nameof( State.ReplaysFolder ) ) )
                OnReplayFolderChanged( );
            else if( e.PropertyName.Equals( nameof( State.TracklineFile ) ) )
                OnTracklinesLoaded( state.Track,state.TracklineFile );
        }
    }
    private void Timeline_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( sender is not null and VMTimeline timeline )
        {
            switch( e.PropertyName )
            {
                case nameof( VMTimeline.CurrentFrame ):
                    OnTimelineChange( timeline,TimelineChangeType.CurrentFrame );
                    break;
                case nameof( VMTimeline.IsActive ):
                    OnTimelineChange( timeline,TimelineChangeType.IsActive );
                    break;
                case nameof( VMTimeline.ScrubA ):
                case nameof( VMTimeline.ScrubB ):
                    OnTimelineChange( timeline,TimelineChangeType.Scrubs );
                    break;
            }
        }
    }
}