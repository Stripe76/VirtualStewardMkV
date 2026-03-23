using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.Realtime.ViewModels;

public class VMRealtimeTimeline : VMRealtime
{
  private readonly VMTimeline _timeline;
  private readonly VMFrameValidation _frameValidation;

  private VMReplay _replay;
  
  private uint _frameOffset;
  private double _replayFrequency;

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
  public VMFrameValidation FrameValidation => _frameValidation;

  public VMRealtimeTimeline( VMReplay replay,VMTimeline timeline,VMFrameValidation frameValidation,uint replayFrequencyMs ) : base( replayFrequencyMs )
  {
    _replay = replay;
    _replayFrequency = _replay.ReplayFrequency;

    _timeline = timeline;
    _frameValidation = frameValidation; 

    FrequencyMs = (uint)(1000 / _replay.ReplayFrequency);
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
      int frame = (int)(_frameOffset + ((milliseconds * PlaySpeed / PlaySlowMotion) / _replayFrequency));
      uint validatedFrame = _frameValidation.ValidateFrame( frame ); 

      if( validatedFrame != frame )
      {
        _frameOffset = validatedFrame;

        TimeOffset = (uint)TimeSource.ElapsedMilliseconds;
      }
      _timeline.CurrentFrame = validatedFrame;
    }
  }

  public override bool CanPlay( )
  {
    return _replay.IsLoaded;
  }
}
