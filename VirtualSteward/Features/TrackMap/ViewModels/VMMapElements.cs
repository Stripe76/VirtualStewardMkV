using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Framework.UI;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public abstract class VMMapElement : UIBase
{
  protected double _zoom = 1.0f;
  protected Point _offset = new Point( );
  protected Rect? _clippingRect = null;

  public void SetZoom( double zoom,bool update = true )
  {
    if( _zoom == zoom )
      update = false;

    _zoom = zoom;

    if( update )
      Update( );
  }
  public void SetZoomAndClipping( double zoom,Point offset,Rect clipping,bool update = true )
  {
    if( _zoom == zoom && _offset == offset && _clippingRect == clipping )
      update = false;

    _zoom = zoom;
    _offset = offset;
    _clippingRect = clipping;

    if( update )
      Update( );
  }

  protected virtual void Update( )
  {

  }

  public abstract void AddElements( Avalonia.Controls.Controls elements );
}

public abstract class VMMapElementPosition : VMMapElement
{
  protected double _xPosition,_yPosition;

  protected override void Update( )
  {
    SetPosition( _xPosition,_yPosition );
  }

  public abstract void SetPosition( double x,double y );
}

public abstract class VMMapElementPositionRotation : VMMapElementPosition
{
  protected double _rotation;

  public abstract void SetPositionAndRotation( double x,double y );
}
