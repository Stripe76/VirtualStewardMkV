using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

using Framework.Bindables;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public class VMMapLine : VMMapElement
{
  private double _lineThickness = 1;
  private IImmutableSolidColorBrush _lineColor = Brushes.Black;

  public double LineThickness
  {
    get => _lineThickness;
    set
    {
      if( SetProperty( ref _lineThickness,value ) )
      {
        foreach( var (Points, Lines) in _lines )
          foreach( var poly in Lines )
            poly.StrokeThickness = _lineThickness;
      }
    }
  }
  public IImmutableSolidColorBrush LineColor
  {
    get => _lineColor;
    set
    {
      if( SetProperty( ref _lineColor,value ) )
      {
        foreach( var (Points, Lines) in _lines )
          foreach( var poly in Lines )
            poly.Stroke = _lineColor;
      }
    }
  }

  public Shape? StartCap = null;
  public Shape? EndCap = null;

  public Point StartCapPosition
  {
    set
    {
      if( StartCap != null )
      {
        Canvas.SetLeft( StartCap,value.X - StartCap.Width / 2 );
        Canvas.SetTop( StartCap,value.Y - StartCap.Height / 2 );
      }
    }
  }
  public Point EndCapPosition
  {
    set
    {
      if( EndCap != null )
      {
        Canvas.SetLeft( EndCap,value.X - EndCap.Width / 2 );
        Canvas.SetTop( EndCap,value.Y - EndCap.Height / 2 );
      }
    }
  }

  private readonly List<(PointCollection Points,List<Polyline> Lines)> _lines = [];

  public VMMapLine( PointCollection points,double zoom,Rect clippingRect )
  {
    _zoom = zoom;
    _clippingRect = clippingRect;

    AddLine( points );
  }
  public VMMapLine( List<PointCollection> pointsLists,double zoom,Rect clippingRect,bool setStartCap = false,bool setEndCap = false )
  {
    _zoom = zoom;
    _clippingRect = clippingRect;

    foreach( var points in pointsLists )
      AddLine( points );

    if( setStartCap && pointsLists.Count > 0 && pointsLists[0].Count > 0 )
      StartCapPosition = pointsLists[0][0];

    if( setEndCap && pointsLists.Count > 0 && pointsLists[^1].Count > 0 )
      EndCapPosition = pointsLists[^1][^1];
  }

  public override void AddElements( Avalonia.Controls.Controls elements )
  {
    foreach( var (Points, Lines) in _lines )
    {
      foreach (var poly in Lines)
      {
        if (poly.Parent != null)
          ((Canvas)poly.Parent).Children.Remove(poly);
          
        elements.Add(poly);
      }
    }
  }

  protected override void Update( )
  {
    for( int p = 0; p < _lines.Count; p++ )
      _lines[p] = new( _lines[p].Points,CreatePolylines( VMMap.GetPolylinePointsClipped( _lines[p].Points,_zoom,_offset,_clippingRect,false ) ) );

    /*
    if( _lines.Count > 1 )
    {
      if( _lines[0].Line.Points.Count > 0 && _lines[^1].Line.Points.Count > 0 )
      {
        StartCapPosition = _lines[0].Line.Points[0];
        EndCapPosition = _lines[^1].Line.Points[^1];
      }
    }
    else if( _lines.Count > 0 && _lines[0].Line != null && _lines[0].Line.Points.Count > 0 )
    {
      StartCapPosition = _lines[0].Line.Points[0];
      EndCapPosition = _lines[0].Line.Points[^1];
    }
    */
  }

  private void AddLine( PointCollection points )
  {
    _lines.Add( new( points,CreatePolylines( VMMap.GetPolylinePointsClipped( points,_zoom,_offset,_clippingRect,false ) ) ) );
  }

  private List<Polyline> CreatePolylines( List<PointCollection> polylinesPoints )
  {
    List<Polyline> polylines = [];
    foreach( var pts in polylinesPoints )
    {
      polylines.Add( new Polyline( ) { Stroke = _lineColor,StrokeThickness = _lineThickness/*,StrokeDashArray = _lineDashArray*/,Points = pts } );
    }
    return polylines;
  }
}
