using System;
using System.Diagnostics;
using Avalonia.Threading;

using Framework.UI;

namespace VirtualSteward.Features.Realtime.ViewModels;

public abstract class VMRealtime : UIItem
{
  private DispatcherTimer? _timerPlay = null;
  private readonly Stopwatch _timeSource = new ( );

  private bool _isPlaying = false;
  private int _playSpeed = 1,_playSlowMotion = 1;

  private uint _timeOffset,_lastTick;
  private uint _frequencyMs;

  public Stopwatch TimeSource
  {
    get => _timeSource; 
  }

  public bool IsPlaying
  {
    get => _isPlaying || Playing( );
    set { SetProperty( ref _isPlaying,value ); }
  }

  public int PlaySpeed
  {
    get => _playSpeed;
    set => SetProperty( ref _playSpeed,value );
  }
  public int PlaySlowMotion
  {
    get => _playSlowMotion;
    set => SetProperty( ref _playSlowMotion,value );
  }

  public uint TimeOffset 
  {
    get => _timeOffset;
    set => _timeOffset = value;
  }

  public string PlayingSpeed
  {
    get => string.Format( "Play speed: {0}",(_playSlowMotion >= 2) ? string.Format( "1/{0}",_playSlowMotion ) : string.Format( "{0}x",_playSpeed ) );
  }

  public uint FrequencyMs
  {
    get => _frequencyMs;
    set
    {
      if( SetProperty( ref _frequencyMs, value ) && _timerPlay != null )
      {
        if( _frequencyMs > 0 )
        {
          _timerPlay.Interval = TimeSpan.FromMilliseconds( _frequencyMs );
          _timerPlay.IsEnabled = true;
        }
        else
        {
          _timerPlay.IsEnabled = false;
        }
      }
    }
  }

  public VMRealtime( uint frequencyMs )
  {
    _frequencyMs = frequencyMs;
  }

  public virtual void Play( )
  {
    _timerPlay?.Stop( );;

    _timerPlay = new DispatcherTimer( );
    _timerPlay.Interval = TimeSpan.FromMilliseconds( _frequencyMs );
    _timerPlay.Tick += TimerPlay_Tick;

    if( !_timeSource.IsRunning )
    {
      _timeOffset = 0;

      _timeSource.Restart( );
    }
    else
    {
      _timeOffset = (uint)_timeSource.Elapsed.TotalMilliseconds;
    }
    _timerPlay.Start( );

    IsPlaying = true;

    OnPropertyChanged( nameof( PlayingSpeed ) );
  }
  public virtual void Stop( )
  {
    _timerPlay?.Stop( );
    _timerPlay = null;

    IsPlaying = false;
  }

  public virtual void NextFrame( )
  {
  }
  public virtual void PreviousFrame( )
  {
  }

  public virtual void IncreaseSpeed( )
  {
    if( PlaySlowMotion >= 2 )
    {
      if( PlaySlowMotion > 2 )
        PlaySlowMotion -= 1;
    }
    else
    {
      PlaySpeed += 1;

      if( PlaySpeed == 0 )
        PlaySpeed = 1;
    }
    _timeOffset = (uint)TimeSource.Elapsed.TotalMilliseconds;

    OnPropertyChanged( nameof( PlayingSpeed ) );
  }
  public virtual void DecreaseSpeed( )
  {
    if( _playSlowMotion >= 2 )
      _playSlowMotion += 1;
    else
    {
      _playSpeed -= 1;

      if( PlaySpeed == 0 )
        PlaySpeed = -1;
    }
    _timeOffset = (uint)_timeSource.Elapsed.TotalMilliseconds;

    OnPropertyChanged( nameof( PlayingSpeed ) );
  }

  public virtual bool CanPlay( )
  {
    return !IsPlaying;
  }
  public virtual bool CanStop( )
  {
    return IsPlaying;
  }
  public virtual bool CanPause( )
  {
    return CanPlay( );
  }
  public virtual bool CanNextFrame( )
  {
    return CanPlay( );
  }
  public virtual bool CanPreviousFrame( )
  {
    return CanPlay( );
  }
  public virtual bool CanIncreaseSpeed( )
  {
    return IsPlaying;
  }
  public virtual bool CanDecreaseSpeed( )
  {
    return IsPlaying;
  }

  protected virtual bool Playing( )
  {
    return false;
  }

  protected virtual void RealTimeTick( uint totalElapsed,uint delta )
  {

  }

  private void TimerPlay_Tick( object? sender,EventArgs e )
  {
    if( _isPlaying )
    {
      uint elapsed = (uint)_timeSource.Elapsed.TotalMilliseconds;

      RealTimeTick( elapsed,elapsed-_lastTick );

      _lastTick = elapsed;
    }
  }
}
