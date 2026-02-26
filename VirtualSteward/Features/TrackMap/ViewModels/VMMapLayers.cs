using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using Framework.Bindables;

using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public abstract class VMMapLayer : Canvas
{
  private double _zoom = 1.0f;
  private Point _offset = new ( );
  private Rect _clipping = new ( );

  public double Zoom
  {
    get => _zoom;
    set
    {
      if( _zoom != value )
      {
        _zoom = value;

        UpdateLayer( );
      }
    }
  }
  public Point Offset
  {
    get => _offset;
    set
    {
      if( _offset != value )
      {
        _offset = value;

        UpdateLayer( );
      }
    }
  }
  public Rect Clipping
  {
    get => _clipping;
    set
    {
      if( _clipping != value )
      {
        _clipping = value;

        UpdateLayer( );
      }
    }
  }

  protected abstract void UpdateLayer( );

  protected void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    if( sender != null && e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset )
    {
      UpdateLayer( );
    }
  }
}

public class VMMapLayerList : ObservableCollectionEx<VMMapLayer>
{

}
