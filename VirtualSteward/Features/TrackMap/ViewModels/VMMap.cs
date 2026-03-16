using System.Collections.Generic;
using Avalonia;
using Framework.Bindables;
using Framework.UI;
using VirtualSteward.Features.TrackMap.EditingTools;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public class VMMap( bool defaultMapMove ) : UIBase
{
  #region Fields
  private Point _centerOn,_currentMousePosition;
  private string _debugString = "";

  private double _zoom = 1.0f;
  private Point _offset = new ( );
  private Rect _clipping = new ( ),_display = new Rect( );

  private EditingTool? _editingTool = null;

  private bool _enableDefaultMapMove = defaultMapMove;
  #endregion

  #region Properties
  public double Zoom
  {
    get => _zoom;
    set
    {
      if( SetProperty( ref _zoom,value ) )
      {
        UpdateLayers();
      }
      OnPropertyChanged( nameof( DebugString ) );
    }
  }
  public Point Offset
  {
    get => _offset;
    set
    {
      value = new Point(CanvasToTrackX(value.X, _zoom), CanvasToTrackY(value.Y, _zoom));

      if( SetProperty( ref _offset,value ) )
      {
        UpdateLayers();
      }
    }
  }
  public Rect Clipping
  {
    get => _clipping;
    set
    {
      value = new Rect( CanvasToTrackX( value.X,_zoom ),
                        CanvasToTrackY( value.Y,_zoom ),
                        value.Width / _zoom,
                        value.Height / _zoom );

      if( SetProperty( ref _clipping,value ) )
      {
        UpdateLayers();
      }
    }
  }
  
  public Rect Display
  {
    get => _display;
    set => SetProperty( ref _display,value );
  }

  public Point CenterOn
  {
    get => _centerOn;
    set
    {
      _centerOn = value;

      OnPropertyChanged( nameof( CenterOn ) );
    }
  }

  public EditingTool? EditingTool
  {
    get => _editingTool;
  }

  public bool EnableDefaultMapMove
  {
    get => _enableDefaultMapMove;
  }

  public VMMapLayerList Layers { get; } = []; 
    
  public Point CurrentMousePosition
  {
    get => _currentMousePosition;
    set
    {
      _currentMousePosition = new Point( value.X,value.Y );

      //OnPropertyChanged( nameof( MouseX ) );
      //OnPropertyChanged( nameof( MouseY ) );
      //OnPropertyChanged( nameof( MouseZ ) );

      OnPropertyChanged( nameof( DebugString ) );
      OnPropertyChanged( nameof( CurrentMousePosition ) );
    }
  }

  public string DebugString
  {
    get => _debugString;
    set => SetProperty(ref _debugString, value);
  }
  #endregion

  public void AddLayer( VMMapLayer layer,bool first = false )
  {
    if( first )
      Layers.Insert( 0,layer );
    else
      Layers.Add( layer );
  }

  public void UpdateLayers()
  {
    foreach (var layer in Layers)
    {
      layer.UpdateLayer(_zoom,_offset,_clipping);
    }
  }

  public static List<PointCollection> GetPolylinePointsClipped( PointCollection line,double zoom,Point offset,Rect? clippingRect = null,bool bCloseLoop = true )
  {
    PointCollection? points = null;
    List<PointCollection> lines = [];

    if( clippingRect != null )
    {
      //clippingRect.Inflate( clippingRect.Width * 0.1f,clippingRect.Height * 0.1f );

      for( int i = 0; i < line.Count; i++ )
      {
        Point point = line[i];
        if( clippingRect.Value.Contains( point ) )
        {
          points ??= [];
          points.Add( new Point(offset.X + line[i].X, offset.Y + line[i].Y) );

          if( bCloseLoop && i == line.Count - 1 )
          {
            points.Add( new Point( offset.X + line[0].X,offset.Y + line[0].Y ) );
            points.Add( new Point( offset.X + line[1].X,offset.Y + line[1].Y ) );
          }
        }
        else
        {
          if( points != null )
            lines.Add( points );
          points = null;
        }
      }
      if( points != null )
        lines.Add( points );
    }
    else
    {
      points = [];
      for( int i = 0; i < line.Count; i++ )
      {
        points.Add(new Point(offset.X + line[i].X, offset.Y + line[i].Y));
      }
      if( points != null )
        lines.Add( points );
    }
    points = null;

    List<PointCollection> polylines = [];
    foreach( PointCollection currentLine in lines )
    {
      Point last = new ( 0,0 );
      for( int c = 0; c < currentLine.Count; c++ )
      {
        Point pt = new ( TrackToCanvasX( currentLine[c].X,zoom ),TrackToCanvasY( currentLine[c].Y,zoom ) );
        if( (int)pt.X != (int)last.X || (int)pt.Y != (int)last.Y )
        {
          points ??= [];
          points.Add( pt );

          last = pt;
        }
      }
      if( points != null )
        polylines.Add( points );
      points = null;
    }
    return polylines;
  }

  #region Coordinates conversions
  public Point TrackToCanvas( Point pt )
  {
    return new Point( TrackToCanvasX( _offset.X + pt.X,_zoom ),TrackToCanvasY( _offset.Y + pt.Y,_zoom ) );
  }
  public Point TrackToCanvas( double X, double Y )
  {
    return new Point( TrackToCanvasX( _offset.X + X,_zoom ),TrackToCanvasY( _offset.Y + Y,_zoom ) );
  }

  public static double TrackToCanvasX( double X,double fZoom,double fWidth = 0 )
  {
    return (X * fZoom) - (fWidth / 2);
  }
  public static double TrackToCanvasY( double Y,double fZoom,double fHeight = 0 )
  {
    return (Y * fZoom) - (fHeight / 2);
  }

  public Point CanvasToTrack( Point pt )
  {
    return new Point( CanvasToTrackX( pt.X,_zoom ),CanvasToTrackY( pt.Y,_zoom ) );
  }

  private static double CanvasToTrackX( double X,double fZoom )
  {
    return (X / fZoom);
  }
  private static double CanvasToTrackY( double Y,double fZoom )
  {
    return (Y / fZoom);
  }
  #endregion
}
