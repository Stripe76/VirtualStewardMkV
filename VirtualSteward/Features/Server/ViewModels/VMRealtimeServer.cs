using Avalonia.Media;

using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.Realtime.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.Server.ViewModels;

public class VMRealtimeServer : VMRealtimeTimeline
{
  private readonly VMServer _server;

  public VMRealtimeServer( VMReplay replay,VMTimeline timeline,VMServer server ) : base( replay,timeline,1000 )
  {
    _server = server;

    //_background = Brushes.Aqua;
  }

  public override void Play( )
  {
    uint loopEnd = _loopScrubs ? Timeline.ScrubB : Timeline.End;
    uint loopStart = _loopScrubs ? Timeline.ScrubA : (_loopReplay?0:loopEnd);

    _server.Play( loopStart,loopEnd );

    base.Play( );
  }
  public override void Stop( )
  {
    if( _server != null && _server.IsRunning )
    {
      _server.Stop( );
    }
    base.Stop( );
  }

  protected override bool Playing( )
  {
    return _server.IsPlaying;
  }

  public override bool CanPlay( )
  {
    return _server.IsRunning;
  }
  public override bool CanIncreaseSpeed( )
  {
    return false;
  }
  public override bool CanDecreaseSpeed( )
  {
    return false;
  }

  protected override void RealTimeTick( uint totalElapsed,uint delta )
  {
    //Timeline.SetCurrentFrame( _server.CurrentFrame,false,false );
  }
}
