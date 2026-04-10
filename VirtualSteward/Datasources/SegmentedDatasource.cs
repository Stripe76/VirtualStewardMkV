using System;
using System.Collections.Generic;
using ACLibrary.Replays;
using Avalonia;
using Framework.Helpers;
using VirtualSteward.Datasources.ViewModels;

namespace VirtualSteward.Datasources;

public class SegmentedDatasource( CarDatasource datasource ) : CarDatasource
{
  private CarDatasource _datasource = datasource;

  public SegmentList Segments = new ( );

  public override int Length
  {
    get
    {
      uint length = 0;
      foreach ( var item in Segments )
        length += item.Length;
      return (int)length;
    }
  }

  public override string GetFieldValue( uint frame,string field )
  {
    return _datasource.GetFieldValue( MapFrame( frame ),field );
  }

  public override ReplayTail? GetTailData( )
  {
    return _datasource.GetTailData( );
  }
  public override ReplayCarLap[]? GetCarLaps( )
  {
    return _datasource.GetCarLaps( );
  }
  public override ReplayCarData? GetSaveData( uint frame )
  {
    return _datasource.GetSaveData( MapFrame( frame ) );
  }

  public override uint GetNearestFrame( Point pt,uint startFrame,int before,int after )
  {
    int start = Math.Max( 0,(int)startFrame - (int)before );
    if( before < 0 )
      start = 0;
    int end = Math.Min( Length,(int)startFrame + (int)after );
    if( after < 0 )
      end = Length;

    uint frame = startFrame;
    double min = double.MaxValue;

    for( int i = start; i < end; i++ )
    {
      double d = Mathematics.Distance( pt.X,pt.Y,_datasource.GetCarData(MapFrame((uint)i)).Position.X,_datasource.GetCarData(MapFrame((uint)i)).Position.Y );
      if( d < min )
      {
        frame = (uint)i;
        min = d;
      }
    }
    return frame;
  }

  public override VMCarData? GetCarData( uint frame )
  {
    return _datasource.GetCarData( MapFrame( frame ) );
  }
  public override VMServerData? GetServerData( uint frame,VMServerData? serverData = null )
  {
    return _datasource.GetServerData( MapFrame( frame ) );
  }
  public override VMCarPosition? GetPositionAndRotation( uint frame,VMCarPosition? carPosition = null )
  {
    return _datasource.GetPositionAndRotation( MapFrame( frame ) );
  }

  public override VMCarData? GetCurrentCarData( )
  {
    return _datasource.GetCurrentCarData( );
  }

  public override uint GetLapTime( uint frame )
  {
    return _datasource.GetLapTime( MapFrame( frame ) );
  }

  private uint MapFrame( uint frame )
  {
    return Segments.MapFrame( (int)frame );
  }
}

public class SegmentList : List<Segment>
{
  public uint MapFrame( int frame )
  {
      int count = Count;
      for( int i = 0; i < count; i++ )
      {
        if( frame < this[i].Length )
        {
          if( this[i].Start == this[i].End || frame < 0 )
            return this[i].Start;
          return this[i].Start + (uint)frame;
        }
        frame -= (int)this[i].Length;
      }
      if( count > 0 )
        return this[^1].End;
      return 0;
  }

  public int GetSegmentIndexFromFrame( uint requestedFrame )
  {
    int frame = (int)requestedFrame;

    int count = Count;
    for( int i = 0; i < count; i++ )
    {
      if( frame < this[i].Length )
      {
        return i;
      }
      frame -= (int)this[i].Length;
    }
    if( count > 0 )
      return count-1;
    return -1;
  }
  public uint GetSegmentVirtualStart( uint segment )
  {
    uint count = 0;
    for( int i = 0; i < Count && i < segment; i++ )
    {
      count += this[i].Length;
    }
    return count;
  }
}

public class Segment( uint start,int length )
{
  public uint Start = start;
  public uint End = length < 0 ? start : start+(uint)length;

  public uint Length = (uint)Math.Abs( length );
}