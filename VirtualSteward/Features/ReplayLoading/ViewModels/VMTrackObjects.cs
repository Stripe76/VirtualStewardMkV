using System;
using Framework.UI;
using ACLibrary.Replays;

namespace VirtualSteward.Features.ReplayLoading.ViewModels;

public class VMTrackObjects : UIBase
{
  private TrackObject[]? _data = null;

  public int Length => _data?.Length ?? 0;
  public uint TrackObjectsNumber { get; private set; } = 0;

  public VMTrackObjects( )
  {
    _data = [];
  }
  public VMTrackObjects( TrackObject[]? data, uint trackObjectsNumber )
  {
    SetData( data,trackObjectsNumber );
  }

  public void SetData( TrackObject[]? data, uint trackObjectsNumber )
  {
    _data = data;
    TrackObjectsNumber = trackObjectsNumber;
  }

  public TrackObject GetSaveData( uint frame )
  {
    return _data[Math.Clamp( frame,0,(uint)_data.Length-1 )];
  }

  public bool GetObjectData( uint frame,VMTrackObjectData data )
  {
    if( _data != null && _data.Length > 0 )
    {
      frame = Math.Clamp( frame,0,(uint)_data.Length-1 );

      data.SunAngle = (float)_data[frame].SunAngle1;
      
      return true;
    }
    return false;
  }

  public float GetStartSunAngle( )
  {
    if( _data != null && _data.Length > 0 )
      return (float)_data[0].SunAngle1;
    return 0;
  }
}

public class VMTrackObjectData
{
  internal float SunAngle;
}
