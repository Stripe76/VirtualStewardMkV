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

  private VMRealtimeTimeline _realtimeReplay;

  public FeatureCommandList Commands { get; } = [];

  public bool LoopReplay
  {
    get => _realtimeReplay.LoopReplay;
    set => _realtimeReplay.LoopReplay = value; 
  }
  public bool LoopScrubs
  {
    get => _realtimeReplay.LoopScrubs;
    set => _realtimeReplay.LoopScrubs = value; 
  }

  public Realtime( State state,DataTemplates templates,VMTimeline timeline ) : base( state,templates )
  {
    _realtimeReplay = new VMRealtimeTimeline( state.Replay,timeline,(uint)(1000.0 / state.Replay.ReplayFrequency) );
    
    Commands.Add( new FeatureCommand() { Icon = "\xf2b6;", Text = "Rewind", Tooltip = "Rewind", RoutedCommand = RewindCommand } );
    Commands.Add( new RepeatCommand() { Icon = "\xf2e5;", Text = "Previous frame", Tooltip = "Previous frame", RoutedCommand = PreviousFrameCommand } );
    Commands.Add( _playCommand = new FeatureCommand() { Icon = "\xf29d;", Text = "Play", Tooltip = "Play", RoutedCommand = PlayCommand } );
    Commands.Add( new RepeatCommand() { Icon = "\xf2e6;", Text = "Next frame", Tooltip = "Next frame", RoutedCommand = NextFrameCommand } );
    Commands.Add( new FeatureCommand() { Icon = "\xf1d9;", Text = "Fast forward", Tooltip = "Fast forward", RoutedCommand = FastForwardCommand } );
  }

  public override void AddDataTemplates( DataTemplates templates )
  {
    templates.Add( new FuncDataTemplate<Realtime>( (_,_) => new Controls.Realtime() ) );
    //templates.Add( new FuncDataTemplate<VMRealtime>( (_,_) => new Controls.Realtime() ) );
  }

  public override Feature AddPage(UIBaseList pages, string? headerTitle = null)
  {
    //pages.Add( RealtimeVM );

    return base.AddPage(pages,headerTitle);
  }

  public override void OnReplayChanged( VMReplay replay )
  {
    _realtimeReplay.Stop( );
    _realtimeReplay.Replay = replay;
    //_realtimeReplay = new VMRealtimeTimeline( _state.Replay,_realtimeReplay.Timeline,(uint)(1000.0 / _state.Replay.ReplayFrequency) );
  }

  [RelayCommand] protected void Play()
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
  [RelayCommand] protected void Rewind()
  {
    _realtimeReplay.DecreaseSpeed();
  }
  [RelayCommand] protected void FastForward()
  {
    _realtimeReplay.IncreaseSpeed();
  }
  [RelayCommand] protected void PreviousFrame()
  {
    _playCommand.Icon = "\xf29d;";
    _realtimeReplay.PreviousFrame();
  }
  [RelayCommand] protected void NextFrame()
  {
    _playCommand.Icon = "\xf29d;";
    _realtimeReplay.NextFrame();
  }

  [RelayCommand] protected void SetLoopReplay()
  {
    _realtimeReplay.LoopReplay = !_realtimeReplay.LoopReplay;
    
    OnPropertyChanged(nameof(LoopReplay));
  }
  [RelayCommand] protected void SetLoopScrubs()
  {
    _realtimeReplay.LoopScrubs = !_realtimeReplay.LoopScrubs;
    
    OnPropertyChanged(nameof(LoopScrubs));
  }

  /*
  private void RealtimePlay( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtimeTimeline realtime )
    {
      realtime.PlaySlowMotion = 1;

      realtime.Play( );
    }
  }
  private void RealtimePause( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtimeTimeline realtime )
    {
      realtime.PlaySpeed = 1;
      realtime.PlaySlowMotion = 10;

      if( realtime.IsPlaying )
        realtime.Stop( );
      else
        realtime.Play( );
    }
  }
  private void RealtimeStop( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      realtime.Stop( );
  }
  private void RealtimeRewind( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      realtime.DecreaseSpeed( );
  }
  private void RealtimeFastForward( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      realtime.IncreaseSpeed( );
  }
  private void RealtimeNextTrack( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      realtime.NextFrame( );
  }
  private void RealtimePreviousTrack( object sender,ExecutedRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      realtime.PreviousFrame( );
  }

  private void ReplayLoaded_CanExecute( object sender,CanExecuteRoutedEventArgs e )
  {
    e.CanExecute = _state.Replay.IsLoaded;
  }
  private void ServerNotRunning_CanExecute( object sender,CanExecuteRoutedEventArgs e )
  {
    //e.CanExecute = _state.Replay.IsLoaded && !_state.Server.IsRunning;
    e.CanExecute = _state.Replay.IsLoaded;
  }
  private void IsPlaying_CanExecute( object sender,CanExecuteRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      //e.CanExecute = realtime.IsPlaying || _state.Server.IsRunning;
      e.CanExecute = realtime.IsPlaying;
  }
  private void IsPlayingAndServerNotRunning_CanExecute( object sender,CanExecuteRoutedEventArgs e )
  {
    if( e.Parameter is not null and VMRealtime realtime )
      //e.CanExecute = realtime.IsPlaying && !_state.Server.IsRunning;
      e.CanExecute = realtime.IsPlaying;
  }
  
  */
}
