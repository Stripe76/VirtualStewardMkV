using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.Timelines.ViewModels;

public partial class VMTimeline : UIItem
{
  [ObservableProperty] private bool _showName = true;

  [ObservableProperty] private string _timelineName;
  [ObservableProperty] private VMPlayerList _players;

  private uint _currentFrame = 0;
  public uint CurrentFrame
  {
    get => _currentFrame;
    set => SetCurrentFrame( value,false,true );
  }

  [ObservableProperty] private uint _start = 0,_end = 0,_totalLength = 0;
  [ObservableProperty] private uint _scrubA = 0,_scrubB = 0;

  public VMTimelineMarkerList Markers { get; } = [];

  /*
  private double _replayFrequency = 0f;

  public string CurrentTime
  {
    get => string.Format( "Replay time: {0} ({1})",FrameToTimeString( CurrentFrame,_replayFrequency ),CurrentFrame );
  }
  public string ScrubsTime
  {
    get => ScrubsToTimeString( ScrubA,ScrubB,_replayFrequency );
  }
  */

  public VMTimeline( string name,VMPlayerList players )
  {
    _timelineName = name;

    _players = players;
    _players.CollectionChanged += Players_CollectionChanged;
    _players.ActiveItemChanged += Players_ActiveItemChanged;

    Start = 0;
    End = TotalLength = players.MaxFrames;
    CurrentFrame = 0;
  }

  public void SetCurrentFrame( uint frame,bool smoothing,bool updateServer )
  {
    _currentFrame = frame;

    OnPropertyChanged( nameof( CurrentFrame ) );
    //OnPropertyChanged( nameof( CurrentTime ) );

    if( !IsActive )
      IsActive = true;

    //if( updateServer )
      //UpdateServer( );
    //UpdatePlayerCarsPosition( smoothing && _followPlayer == null );
  }

  private void SetPlayerLapsMarkers( VMPlayer player )
  {
    //using( new BindingListBatchUpdate<VMTimelineMarker>( Markers ) )
    {
      Markers.Clear( );
      
      foreach( VMPlayerLap lap in player.Laps )
      {
        VMTimelineMarker marker = new( lap.LapName,lap.StartFrame )
        {
          //Title = lap.LapName,
          Start = Start,
          End = End,
          StartFrame = lap.StartFrame,
          EndFrame = lap.EndFrame,
          //Position = lap.StartFrame
          //Position = 2048
        };
        Markers.Add( marker );
      }
    }
  }

  private void Players_ActiveItemChanged( object? sender,VMPlayer? e )
  {
    if( Players.ActiveItem != null )
      SetPlayerLapsMarkers( Players.ActiveItem );
  }
  private void Players_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    End = TotalLength = Players.MaxFrames;
  }

  #region Helpers
  public static string FrameToTimeString( uint nTime,double dFrequency,bool bMilli = true )
  {
    uint nMilliSeconds = (uint)(nTime*dFrequency);

    if( nMilliSeconds >= 3600000 )
    {
      if( bMilli )
        return
          $"{nMilliSeconds / 60000 / 60:00}:{nMilliSeconds / 60000:00}:{nMilliSeconds / 1000 % 60:00}:{nMilliSeconds % 1000:000}";
      return $"{nMilliSeconds / 60000 / 60:00}:{nMilliSeconds / 60000:00}:{nMilliSeconds / 1000 % 60:00}";
    }
    if( bMilli )
      return $"{nMilliSeconds / 60000:00}:{nMilliSeconds / 1000 % 60:00}:{nMilliSeconds % 1000:000}";
    return $"{nMilliSeconds / 60000:00}:{nMilliSeconds / 1000 % 60:00}";
  }
  public static string ScrubsToTimeString( uint scrubA,uint scrubB,double frequency,bool milli = true )
  {
    return $"{FrameToTimeString( scrubA,frequency,milli )} - {FrameToTimeString( scrubB,frequency,milli )}";
  }
  #endregion
}

public class VMTimelineList : MultiList<VMTimeline>
{

}
