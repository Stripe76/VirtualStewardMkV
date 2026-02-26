using System.Collections.Generic;
using System.ComponentModel;
using ACLibrary.Tracks;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

using Framework.UI;

using VirtualSteward.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Classes;

public partial class State : ObservableObject
{
    private readonly FilesManager _filesMmanger;
    private readonly SortedList<string,VMTrackInfo> _tracks = [];
    
    [ObservableProperty] private VMReplay _replay = new();
    [ObservableProperty] private VMTrackInfo _track = new( "","" );

    public VMPlayerList Players { get; } = new(true,true);

    public State( FilesManager filesManager)
    {
        _filesMmanger = filesManager;
    }

    public VMTrackInfo GetTrackInfo( string trackID,string variantID,bool setCSPSettingsFile )
    {
        string key = trackID + "_" + variantID;
        if( _tracks.TryGetValue( key,out VMTrackInfo? value ) )
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
                if( !_tracks.ContainsKey( key ) )
                    _tracks.Add( key,newInfo );
            }
            return newInfo;
        }
        return new VMTrackInfo( trackID,variantID );
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