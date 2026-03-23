using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.Templates;

using Framework.UI;

using VirtualSteward.Classes;
using VirtualSteward.Features.Realtime.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.Realtime;

public partial class  Realtime : StateFeature
{
  private readonly FeatureCommand _playCommand;
  private readonly VMRealtimeTimeline _realtimeReplay;
  private readonly VMFrameValidationTimeline _frameValidation;

  public FeatureCommandList Commands { get; } = [];
  public VMFrameValidationTimeline FrameValidation => _frameValidation;

  public Realtime( State state,DataTemplates templates,VMTimeline timeline ) : base( state,templates )
  {
    _realtimeReplay = new VMRealtimeTimeline( state.Replay,timeline,_frameValidation = new VMFrameValidationTimeline( timeline ),(uint)(1000.0 / state.Replay.ReplayFrequency) );
    
    Commands.Add( new FeatureCommand() { Icon = "\xf2b6", Text = "Rewind", Tooltip = "Rewind", RoutedCommand = RewindCommand } );
    Commands.Add( new RepeatCommand() { Icon = "\xf2e5", Text = "Previous frame", Tooltip = "Previous frame", RoutedCommand = PreviousFrameCommand } );
    Commands.Add( _playCommand = new FeatureCommand() { Icon = "\xf29d", Text = "Play", Tooltip = "Play", RoutedCommand = PlayCommand } );
    Commands.Add( new RepeatCommand() { Icon = "\xf2e6", Text = "Next frame", Tooltip = "Next frame", RoutedCommand = NextFrameCommand } );
    Commands.Add( new FeatureCommand() { Icon = "\xf1d9", Text = "Fast forward", Tooltip = "Fast forward", RoutedCommand = FastForwardCommand } );
  }

  public override Feature AddDataTemplates( DataTemplates templates )
  {
    templates.Add( new FuncDataTemplate<Realtime>( (_,_) => new Controls.Realtime() ) );
    //templates.Add( new FuncDataTemplate<VMRealtime>( (_,_) => new Controls.Realtime() ) );
    return this;
  }

  public override void OnReplayChanged( VMReplay replay )
  {
    _playCommand.Icon = "\xf29d;";
    _realtimeReplay.Stop( );
    _realtimeReplay.PlaySpeed = 1;
    _realtimeReplay.PlaySlowMotion = 1;
    _realtimeReplay.Replay = replay;
    //_realtimeReplay = new VMRealtimeTimeline( _state.Replay,_realtimeReplay.Timeline,(uint)(1000.0 / _state.Replay.ReplayFrequency) );
  }

  [RelayCommand] private void Play()
  {
    if (_realtimeReplay.IsPlaying)
    {
      _playCommand.Icon = "\xf29d;";
      _realtimeReplay.Stop();
    }
    else
    {
      _playCommand.Icon = "\xf28a";
      _realtimeReplay.Play();
    }
  }
  [RelayCommand] private void Rewind()
  {
    _realtimeReplay.DecreaseSpeed();
  }
  [RelayCommand] private void FastForward()
  {
    _realtimeReplay.IncreaseSpeed();
  }
  [RelayCommand] private void PreviousFrame()
  {
    _playCommand.Icon = "\xf29d;";
    _realtimeReplay.PreviousFrame();
  }
  [RelayCommand] private void NextFrame()
  {
    _playCommand.Icon = "\xf29d;";
    _realtimeReplay.NextFrame();
  }

  [RelayCommand] private void SetLoopReplay()
  {
    _frameValidation.LoopReplay = !_frameValidation.LoopReplay;
  }
  [RelayCommand] private void SetLoopScrubs()
  {
    _frameValidation.LoopScrubs = !_frameValidation.LoopScrubs;
  }
}
