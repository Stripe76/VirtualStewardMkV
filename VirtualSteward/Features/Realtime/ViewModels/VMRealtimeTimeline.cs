using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.Realtime.ViewModels;

public class VMRealtimeTimeline : VMRealtime
{
  private VMReplay _replay;
  private readonly VMTimeline _timeline;

  private int _carSmoothing = 1;
  private double _replayFrequency;

  private bool _isUpdatingFrame = false;
  protected bool _loopReplay = false,_loopScrubs = false;

  private uint _frameOffset,_loopStart = 0,_loopEnd = 0;

  public VMReplay Replay
  {
    get => _replay;
    set
    {
      _replay = value;
      _replayFrequency = _replay.ReplayFrequency;

      FrequencyMs = (uint)(1000 / _replay.ReplayFrequency);
    }
  }
  public VMTimeline Timeline
  {
    get => _timeline;
  }

  public uint FrameOffset
  {
    get => _frameOffset;
  }

  public int MovementSmoothing
  {
    get => _carSmoothing;
    set => SetProperty( ref _carSmoothing,value );
  }

  public bool LoopReplay
  {
    get => _loopReplay;
    set
    {
      if( SetProperty( ref _loopReplay,value ) )
        UpdateLoopFrames( );
    }
  }
  public bool LoopScrubs
  {
    get => _loopScrubs;
    set
    {
      if( SetProperty( ref _loopScrubs,value ) )
        UpdateLoopFrames( );
    }
  }

  public VMRealtimeTimeline( VMReplay replay,VMTimeline timeline,uint replayFrequencyMs ) : base( replayFrequencyMs )
  {
    _replay = replay;
    _replayFrequency = _replay.ReplayFrequency; 

    _timeline = timeline;
    _timeline.PropertyChanged += Timeline_PropertyChanged;

    FrequencyMs = (uint)(1000 / _replay.ReplayFrequency);
  }

  public override void Play( )
  {
    //PlaySlowMotion = 1;

    //_frameOffset = startFrame;

    base.Play( );
  }
  public override void Stop( )
  {
    base.Stop( );

    _timeline.CurrentFrame = _timeline.CurrentFrame;
  }

  public override void NextFrame( )
  {
    Stop( );

    _timeline.CurrentFrame++;
  }
  public override void PreviousFrame( )
  {
    Stop( );

    _timeline.CurrentFrame--;
  }

  public override void IncreaseSpeed( )
  {
    _frameOffset = _timeline.CurrentFrame;

    base.IncreaseSpeed( );
  }
  public override void DecreaseSpeed( )
  {
    _frameOffset = _timeline.CurrentFrame;

    base.DecreaseSpeed( );
  }

  protected override void RealTimeTick( uint totalElapsed,uint delta )
  {
    uint milliseconds = totalElapsed - TimeOffset;

    if( IsPlaying )
    {
      uint nFrame = (uint)(_frameOffset + ((milliseconds * PlaySpeed / PlaySlowMotion) / _replayFrequency));

      _isUpdatingFrame = true;
      _timeline.SetCurrentFrame( ValidateFrame( nFrame ),MovementSmoothing > 1 && PlaySpeed == 1 && PlaySlowMotion == 1,false );
      _isUpdatingFrame = false;
    }
  }

  public override bool CanPlay( )
  {
    return _replay.IsLoaded;
  }

  private void UpdateLoopFrames( )
  {
    _loopEnd = _loopScrubs ? (_loopReplay ? _timeline.ScrubB : _timeline.End) : _timeline.End;
    _loopStart = _loopScrubs ? (_loopReplay ? _timeline.ScrubA : _loopEnd) : (_loopReplay ? 0 : _loopEnd);

    //_server?.SetLoopFrames( _loopStart,_loopEnd );
  }

  private uint ValidateFrame( uint frame )
  {
    uint resultFrame = frame;
    if( _loopStart == _loopEnd )
    {
      if( resultFrame < 0 )
        resultFrame = 0;
      else if( resultFrame >= _loopEnd )
        resultFrame = _loopEnd;
    }
    else
    {
      if( resultFrame < _loopStart )
        resultFrame = _loopEnd;
      else if( resultFrame >= _loopEnd )
        resultFrame = _loopStart;
    }
    if( resultFrame != frame )
    {
      _frameOffset = resultFrame;

      TimeOffset = (uint)TimeSource.ElapsedMilliseconds;
    }
    return resultFrame;
  }

  private void Timeline_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
  {
    if( !_isUpdatingFrame && e.PropertyName == nameof( VMTimeline.CurrentFrame ) )
    {
      //if( TrafficManager == null )
      {
        TimeOffset = (uint)TimeSource.Elapsed.TotalMilliseconds;

        _frameOffset = _timeline.CurrentFrame;
      }
    }
    if( e.PropertyName == nameof( VMTimeline.ScrubA ) || e.PropertyName == nameof( VMTimeline.ScrubB ) )
    {
      UpdateLoopFrames( );
    }
  }
}
