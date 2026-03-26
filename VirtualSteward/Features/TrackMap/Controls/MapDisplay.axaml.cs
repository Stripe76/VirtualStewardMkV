using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using VirtualSteward.Features.TrackMap.EditingTools;
using VirtualSteward.Features.TrackMap.ViewModels;
using Key = Avalonia.Input.Key;
using MouseButton = Avalonia.Input.MouseButton;

namespace VirtualSteward.Features.TrackMap.Controls;

public partial class MapDisplay : UserControl
{
  private VMMap _map = new ( false );

  private Point _offset = new Point(0,0),_selectedPoint = default;

  private EditingTool? _editingTool;
  private EditingTool _mapMoveEditingTool;
  private readonly EditingTool _defaultEditingTool;

  public Point Offset
  {
    get => _offset;
    set
    {
      _offset = value;

      if( _map != null )
      {
        _map.Offset = _offset;
        
        _map.Clipping = new Rect( -_offset.X,-_offset.Y,Bounds.Size.Width,Bounds.Size.Height );
        _map.Display = new Rect( 0,0,Bounds.Size.Width,Bounds.Size.Height );
      }
      //trackCanvas.RenderTransform = new TranslateTransform(_offset.X,_offset.Y);
      
      //trackTranslate.X = _offset.X;
      //trackTranslate.Y = _offset.Y;

      //backgroundTranslate.X = (_offset.X % 32)-32;
      //backgroundTranslate.Y = (_offset.Y % 32)-32;

      /*
      var translate_x = new DoubleAnimation( )
      {
        From = _offset.X,
        To = value.X,
        Duration = TimeSpan.FromMilliseconds( 300 ),
      };
      var translate_y = new DoubleAnimation()
      {
        From = _offset.Y,
        To = value.Y,
        Duration = TimeSpan.FromMilliseconds( 300 ),
      };
      trackTranslate.BeginAnimation( TranslateTransform.XProperty,translate_x,HandoffBehavior.SnapshotAndReplace );
      trackTranslate.BeginAnimation( TranslateTransform.YProperty,translate_y,HandoffBehavior.SnapshotAndReplace );

      _offset = value;

      _map.Clipping = new Rect( -_offset.X,-_offset.Y,ActualWidth,ActualHeight );
      _map.Display = new Rect( 0,0,ActualWidth,ActualHeight );

      backgroundTranslate.X = (_offset.X % 32)-32;
      backgroundTranslate.Y = (_offset.Y % 32)-32;
      */
    }
  }
  public Point SelectedPoint
  {
    get => _selectedPoint;
    set
    {
      //_map.SelectedPoint = _selectedPoint = value;
    }
  }

  public EditingTool? EditingTool
  {
    get => _editingTool ?? _defaultEditingTool;
    set => _editingTool = value;
  }

  public MapDisplay( )
  {
    InitializeComponent( );

    _defaultEditingTool = _mapMoveEditingTool = new MapMoveEdit( this );

    DataContextChanged += MapDisplay_DataContextChanged;

#if !DEBUG
    //spDebugString.Visibility = Visibility.Collapsed;
#endif
  }

  private void MapDisplay_DataContextChanged( object? sender,EventArgs e )
  {
    if( DataContext is not null and VMMap map )
    {
      _map = map;
      _map.PropertyChanged += Map_PropertyChanged;

      //if( _map.EnableDefaultMapMove && _mapMoveEditingTool == null )
        //_mapMoveEditingTool = new MapMoveEdit( this );
    }
  }

  private void MapDisplay_SizeChanged( object sender,SizeChangedEventArgs e )
  {
    Point ptCenter = _map.CenterOn;

    double X = VMMap.TrackToCanvasX( ptCenter.X,_map.Zoom );
    double Y = VMMap.TrackToCanvasY( ptCenter.Y,_map.Zoom );

    Offset = new Point( Bounds.Size.Width / 2 - X,Bounds.Size.Height / 2 - Y );
  }
  private void Map_PropertyChanged( object? sender,PropertyChangedEventArgs e )
  {
    if( e.PropertyName != null && e.PropertyName.Equals( nameof( VMMap.CenterOn ) ) )
    {
      Point ptCenter = _map.CenterOn;

      double X = VMMap.TrackToCanvasX( ptCenter.X,_map.Zoom );
      double Y = VMMap.TrackToCanvasY( ptCenter.Y,_map.Zoom );

      Offset = new Point( Bounds.Size.Width / 2 - X,Bounds.Size.Height / 2 - Y );
      //Offset = new Point( Bounds.Size.Width / 2,Bounds.Size.Height / 2 );
    }
    else if( e.PropertyName != null && e.PropertyName.Equals( nameof( VMMap.EditingTool ) ) )
    {
      _editingTool = _map.EditingTool;
    }
    /*
    if( e.PropertyName != null && e.PropertyName.Equals( nameof( GUIMap.ShowTrackModel ) ) )
    {
      modelScale.ScaleX = modelScale.ScaleY = Map.Zoom;

      modelTranslate.X = GUIMap.TrackToCanvasX( Map.TrackModelOffset.X,Map.Zoom );
      modelTranslate.Y = GUIMap.TrackToCanvasY( Map.TrackModelOffset.Y,Map.Zoom );
    }
    */
  }

  #region Track map mouse events
  private bool bMouseCapture = false;

  private void Track_MouseDown( object sender,PointerPressedEventArgs args )
  {                              
    if( EditingTool != null )
    {
      bool bCapture = false;

      PointerPoint currentPoint = args.GetCurrentPoint( this ); 
      Point ptMouse = currentPoint.Position;
      Point ptTrack = ScreenToTrack( ptMouse,_map.Zoom );
#if DEBUG
      _map.DebugString = $"{ptMouse.X:0}:{ptMouse.Y:0} ({ptTrack.X:0}:{ptTrack.Y:0}) - {_offset.X:0}:{_offset.Y:0} ({_map.Offset.X:0}:{_map.Offset.Y:0})";
#endif
      if( currentPoint.Properties.IsLeftButtonPressed )
        bCapture = EditingTool.LeftMouseDown( ptMouse,ptTrack );
      else if( currentPoint.Properties.IsRightButtonPressed )
        bCapture = EditingTool.RightMouseDown( ptMouse,ptTrack );
      else if( currentPoint.Properties.IsMiddleButtonPressed )
        _map.CenterOn = new Point( ptTrack.X,ptTrack.Y );

      if( !bCapture )
      {
        if( currentPoint.Properties.IsLeftButtonPressed )
          bCapture = _mapMoveEditingTool.LeftMouseDown( ptMouse,ptTrack );
        else if( currentPoint.Properties.IsRightButtonPressed )
          bCapture = _mapMoveEditingTool.RightMouseDown( ptMouse,ptTrack );
      }
      if( bCapture )
      {
        bMouseCapture = true;
        
        args.Pointer.Capture( this );
      }
    }
  }
  private void Track_MouseUp( object sender,PointerReleasedEventArgs args )
  {
    if( bMouseCapture )
    {
      if( EditingTool != null )
      {
        PointerPoint currentPoint = args.GetCurrentPoint( this ); 
        Point ptMouse = currentPoint.Position;
        Point ptTrack = ScreenToTrack( ptMouse,_map.Zoom );

        if( bMouseCapture )
        {
          if( args.InitialPressMouseButton == MouseButton.Left )
          {
            if( EditingTool.LeftMouseUp( ptMouse,ptTrack ) )
              EditingTool = _defaultEditingTool;
          }
          else if( args.InitialPressMouseButton == MouseButton.Right )
          {
            if( EditingTool.RightMouseUp( ptMouse,ptTrack ))
              EditingTool = _defaultEditingTool;
          }
        }
      }
      bMouseCapture = false;

      args.Pointer.Capture( this );
    }
  }
  private void Track_MouseMove( object sender,PointerEventArgs args )
  {
    PointerPoint currentPoint = args.GetCurrentPoint( this ); 
    Point ptMouse = currentPoint.Position;
    Point ptTrack = ScreenToTrack( ptMouse,_map.Zoom );

    _map.CurrentMousePosition = ptTrack;

    if( EditingTool != null )
    {
      if( bMouseCapture )
      {
        if( currentPoint.Properties.IsLeftButtonPressed && !EditingTool.LeftMouseMove( ptMouse,ptTrack ) )
          _mapMoveEditingTool.LeftMouseMove( ptMouse,ptTrack );
        else if( currentPoint.Properties.IsRightButtonPressed && !EditingTool.RightMouseMove( ptMouse,ptTrack ) )
          _mapMoveEditingTool.RightMouseMove( ptMouse,ptTrack );
      }
      else
      {
        EditingTool.MouseMove( ptMouse,ptTrack );
      }
    }
    #if DEBUG
    _map.DebugString = $"{ptMouse.X:0}:{ptMouse.Y:0} ({ptTrack.X:0}:{ptTrack.Y:0}) - {_offset.X:0}:{_offset.Y:0} ({_map.Offset.X:0}:{_map.Offset.Y:0})";
    #endif
  }
  
  protected void Track_MouseWheel( object sender,PointerWheelEventArgs args )
  {
    double dZoomLastX = ScreenToTrackX( Bounds.Size.Width / 2,_map.Zoom );
    double dZoomLastY = ScreenToTrackY( Bounds.Size.Height / 2,_map.Zoom );
    
    if( args.Delta.Y < 0 )
    {
      if( (args.KeyModifiers & KeyModifiers.Shift) != 0 )
        _map.Zoom *= .4f;
      else
        _map.Zoom *= .8f;
    }
    else
    {
      if( (args.KeyModifiers & KeyModifiers.Shift) != 0 )
        _map.Zoom *= 1.6f;
      else
        _map.Zoom *= 1.2f;
    }
    double X = VMMap.TrackToCanvasX( dZoomLastX,_map.Zoom );
    double Y = VMMap.TrackToCanvasY( dZoomLastY,_map.Zoom );

    Offset = new Point( Bounds.Size.Width / 2 - X,Bounds.Size.Height / 2 - Y );
  }
  #endregion

  #region Coordinates conversions
  private Point ScreenToTrack( Point pt, double fZoom )
  {
    return new Point( ScreenToTrackX( pt.X,fZoom ),ScreenToTrackY( pt.Y,fZoom ) );
  }

  private double ScreenToTrackX( double screenX,double fZoom )
  {
    return ((-_offset.X + screenX) ) / fZoom;
  }
  private double ScreenToTrackY( double screenY,double fZoom )
  {
    return ((-_offset.Y + screenY) ) / fZoom;
  }
  private double TrackToScreenX( double X,double fZoom,double fWidth = 0 )
  {
    return (_offset.X) - (fWidth / 2) + (X * fZoom);
  }
  private double TrackToScreenY( double Y,double fZoom,double fHeight = 0 )
  {
    return (_offset.Y) - (fHeight / 2) + (Y * fZoom);
  }
  #endregion
}