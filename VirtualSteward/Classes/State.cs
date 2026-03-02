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
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Classes;

public partial class State : ObservableObject
{
    private readonly FilesManager _filesMmanger;

    private readonly SortedList<string,CarInfo> _cars = [];
    private readonly SortedList<string,TrackInfo> _tracks = [];
    
    [ObservableProperty] private VMReplay _replay = new();
    [ObservableProperty] private VMCarInfo _car = new( "" );
    [ObservableProperty] private VMTrackInfo _track = new( "","" );

    public VMPlayerList Players { get; } = new(true,true);

    public State( FilesManager filesManager)
    {
        _filesMmanger = filesManager;
    }

    public CarInfo GetCarInfo( string carID )
    {
        if( _cars.TryGetValue( carID,out CarInfo? value ) )
            return value;
        
        CarInfo? info = CarInfo.LoadCarInfo( _filesMmanger.ACCarsFolder,carID );
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
        TrackInfo? info = TrackInfo.LoadTrackInfo( _filesMmanger.ACTracksFolder,trackID,variantID );
        if( info != null )
        {
            VMTrackInfo newInfo = new ( info,_filesMmanger.ACTracksFolder );
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
    protected readonly State _state;

    public StateFeature( State state, string headerTitle = "" ) : base(null,headerTitle)
    {
        _state = state;
        _state.PropertyChanged += State_PropertyChanged;
    }
    public StateFeature( State state,DataTemplates templates, string headerTitle = "" ) : base(templates,headerTitle)
    {
        AddDataTemplates( templates );

        _state = state;
        _state.PropertyChanged += State_PropertyChanged;
    }

    public virtual void OnReplayChanged( VMReplay replay )
    {
    }
    public virtual void OnTrackChanged( VMTrackInfo trackInfo )
    {
    }
    public virtual void OnTracklinesLoaded( VMTrackInfo trackInfo,VMTracklineFileList tracklinesFiles )
    {
    }

    private void State_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
    {
        if( sender is not null and State state && e.PropertyName != null )
        {
            if( e.PropertyName.Equals( nameof( State.Replay ) ) )
                OnReplayChanged( state.Replay );
            else if( e.PropertyName.Equals( nameof( State.Track ) ) )
                OnTrackChanged( state.Track );
            //else if( e.PropertyName.Equals( nameof( State.TracklinesLoaded ) ) )
              //  OnTracklinesLoaded( state.CurrentTrack,state.TracklineFiles );
        }
    }
}