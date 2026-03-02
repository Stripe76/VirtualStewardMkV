using Avalonia.Controls;
using VirtualSteward.Classes;
using VirtualSteward.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.CurrentReplay;

public class CurrentReplay( State state,Window window ) :  StateFeature( state )
{
  private readonly string _windowTitle = window?.Title??"";

  public string CarName => _state.Car.Model;
  public string TrackName => _state.Track.TrackName;

  public override void OnReplayChanged( VMReplay replay )
  {
    _state.Car = new VMCarInfo( _state.GetCarInfo( replay.CarID ) );
    _state.Track = new VMTrackInfo( _state.GetTrackInfo( replay.TrackID,replay.TrackVariantID,true ) );

    OnPropertyChanged( nameof( CarName ) );
    OnPropertyChanged( nameof( TrackName ) );

    if( _state.Replay.IsLoaded )
      window.Title = $"{TrackName} - {CarName} - {_windowTitle}";
    else
      window.Title = _windowTitle;
  }
}
