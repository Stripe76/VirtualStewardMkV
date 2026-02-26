using System;
using System.Numerics;
using System.Collections.Generic;

using Avalonia.Media;

using ACLibrary.Tracklines;
using Avalonia;
using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Tracklines.ViewModels;

public class VMTrackline : UIItem
{
  private readonly double _totalLength,_averageFrameLength;

  private readonly VMTracklineDataList _lineData = [];

  private double _lineThickness = 1.0f;
  private IImmutableSolidColorBrush _lineColor = Brushes.DarkGray;

  public double TotalLength
  {
    get => _totalLength;
  }
  public double AverageFrameLength
  {
    get => _averageFrameLength;
  }

  public double LineThickness
  {
    get => _lineThickness;
    set
    {
      if( SetProperty( ref _lineThickness,value ) && IsSelected && MapLine != null )
        MapLine.LineThickness = value;
    }
  }
  public IImmutableSolidColorBrush LineColor
  {
    get => _lineColor;
    set
    {
      if( SetProperty( ref _lineColor,value ) && IsSelected && MapLine != null )
        MapLine.LineColor = value;
    }
  }

  public string FileName { get; set; }
  public string FileFullPath { get; set; }

  public int Left;
  public int Right;
  public int Top;
  public int Bottom;

  public int Width
  {  
    get => Right - Left;
  }
  public int Height
  {
    get => Bottom - Top;
  }

  public VMTracklineDataList Data
  {
    get => _lineData;
  }

  public VMMapLine? MapLine = null;

  public VMTracklineData this[int index]
  {
    get => _lineData[Math.Clamp(index,0,_lineData.Count-1)];
  }

  public VMTrackline( string lineName,VMTracklineDataList data,bool updateLimits )
  {
    FileName = lineName;
    FileFullPath = "";

    _lineData.AddRange( data );

    _totalLength = _lineData.TotalLength;
    _averageFrameLength = _totalLength / _lineData.Count;

    if( updateLimits )
    {
      foreach( var pos in _lineData )
      {
        if( pos.Position.X < Left )
          Left = (int)pos.Position.X;
        if( pos.Position.X > Right )
          Right = (int)pos.Position.X;
        if( pos.Position.Y < Top )
          Top = (int)pos.Position.Y;
        if( pos.Position.Y > Bottom )
          Bottom = (int)pos.Position.Y;
      }
    }
  }
  public VMTrackline( string filename,Trackline trackline )
  {
    FileName = System.IO.Path.GetFileName( filename );
    FileFullPath = filename;

    foreach( TracklineData data in trackline.Datas )
    {
      _lineData.Add( new VMTracklineData( data ) );
    }
    _totalLength = _lineData.TotalLength;
    _averageFrameLength = _totalLength / _lineData.Count;
  }

  public PointCollection GetLinePoints( int start,int end,int maxLength = 24000 )
  {
    PointCollection points = [];
    //if( end - start < maxLength )
    {
      for( int i = 0; i < _lineData.Count; i++ )
      {
        Vector3 pos = _lineData[i].Position;

        points.Add( new Point( pos.X,pos.Y ) );
      }
      //if( bCloseCap && arPoints.Count > 0 )
      //arPoints.Add( arPoints[0] );
    }
    return points;
  }

  public override string ToString( )
  {
    return FileName;
  }

  #region Colors
  public static IList<IImmutableSolidColorBrush> LineColors
  {
    get => [.. _lineBrushes];
  }

  private static readonly IImmutableSolidColorBrush[] _lineBrushes = {
    Brushes.Red,
    Brushes.Green,
    Brushes.Blue,
    Brushes.BlueViolet,
    Brushes.MediumTurquoise,
    Brushes.Brown,
    Brushes.Orange,
    Brushes.LimeGreen,
    Brushes.Olive,
    Brushes.Bisque,
    Brushes.Plum,
    Brushes.PowderBlue,
    Brushes.Purple,
    Brushes.Salmon,
    Brushes.SteelBlue,
    Brushes.Goldenrod,
    Brushes.DarkSlateBlue,
    Brushes.Tan,
    Brushes.Khaki,
    Brushes.DeepPink,
  };
  #endregion
}

public class VMTracklineList( bool multiSelect = false ) : MultiList<VMTrackline>( multiSelect,false,false )
{
}

public class VMTracklineData
{
  public int ID;

  public Vector3 Position;

  public float Camber;
  public float Radius;
  public float SideLeft;
  public float SideRight;

  //public float Length;
  //public float Direction;

  public VMTracklineData( Vector3 position )
  {
    Position = position;
  }
  public VMTracklineData( TracklineData data )
  {
    ID = data.ID;

    Camber = data.Camber;
    Radius = data.Radius;
    SideLeft = data.SideLeft;
    SideRight = data.SideRight;

    Position = data.Position;
  }
}

public class VMTracklineDataList : List<VMTracklineData>
{
  public double TotalLength
  {
    get 
    {
      double totalLength = 0;
      int count = Count;
      //for( int i = 0; i < count; i++ )
        //totalLength += this[i].Length;
      return totalLength;
    }
  }
}
