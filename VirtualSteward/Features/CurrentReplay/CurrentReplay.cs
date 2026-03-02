using Avalonia.Controls;
using VirtualSteward.Classes;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.ViewModels;

namespace VirtualSteward.Features.CurrentReplay;

public class CurrentReplay( State state,Window window ) :  StateFeature( state )
{
  private readonly Window _window = window;
  private readonly string _windowTitle = window?.Title??"";
  /*
  public string CarName
  {
    get => _state.CurrentCar.Model;
  }
  */
  public string TrackName
  {
    get => _state.Track.TrackName;
  }

  public override void OnReplayChanged( VMReplay replay )
  {
    //_state.CurrentCar = _state.GetCarInfo( replay.CarID );
    _state.Track = new VMTrackInfo( _state.GetTrackInfo( replay.TrackID,replay.TrackVariantID,true ) );

    //OnPropertyChanged( nameof( CarName ) );
    OnPropertyChanged( nameof( TrackName ) );

    _window.Title = $"{_state.Track.TrackName} - {_windowTitle}";
  }
}
