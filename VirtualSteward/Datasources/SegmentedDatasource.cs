using System;
using System.Collections.Generic;
using ACLibrary.Replays;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.ViewModels;

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
      foreach ( var item in Segments.Segments )
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

  public override VMCarData? GetCarData( uint frame )
  {
    return _datasource.GetCarData( MapFrame( frame ) );
  }
  public override VMServerData? GetServerData( uint frame,VMServerData? serverData = null )
  {
    return _datasource.GetServerData( MapFrame( frame ) );
  }
  public override VMCarPosition? GetPositionAndRotation( uint frame )
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

public class SegmentList
{
  public List<Segment> Segments = [];

  public uint MapFrame( int frame ) 
  {
    int count = Segments.Count;
    for( int i = 0; i < count; i++ )
    {
      if( frame < Segments[i].Length )
      {
        if( Segments[i].Start == Segments[i].End || frame < 0 )
          return Segments[i].Start;
        return Segments[i].Start + (uint)frame;
      }
      frame -= (int)Segments[i].Length;
    }
    if( count >0 )
      return Segments[^1].End;
    return 0;
  }
}

public class Segment( uint start,int length )
{
  public uint Start = start;
  public uint End = length < 0 ? start : start+(uint)length;

  public uint Length = (uint)Math.Abs( length );
}