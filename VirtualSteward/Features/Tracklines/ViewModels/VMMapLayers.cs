using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Tracklines.ViewModels;

public class VMLayerTrackline : VMMapLayer
{
  private readonly ObservableCollection<VMTrackline> _lines;
  
  public VMLayerTrackline( ObservableCollection<VMTrackline> lines )
  {
    _lines = lines;
    _lines.CollectionChanged += CollectionChanged;
  }

  protected override void UpdateLayer( )
  {
    Children.Clear( );

    foreach( VMTrackline trackline in _lines )
    {
      trackline.MapLine ??= new VMMapLine( trackline.GetLinePoints( 0,0 ),Zoom,Clipping )
      {
        LineColor = trackline.LineColor,
        LineThickness = trackline.LineThickness,
      };
      trackline.MapLine?.SetZoomAndClipping( Zoom,Offset,Clipping );
      trackline.MapLine?.AddElements( Children );
    }
  }
}

public class VMLayerTracklineFile : VMMapLayer
{
  private readonly ObservableCollection<VMTracklineFile> _files;

  public VMLayerTracklineFile( ObservableCollection<VMTracklineFile> files )
  {
    _files = files;
    _files.CollectionChanged += CollectionChanged;
  }

  protected override void UpdateLayer( )
  {
    Children.Clear( );

    foreach( VMTracklineFile file in _files )
    {
      foreach( VMTrackline trackline in file.Lines.SelectedItems )
      {
        trackline.MapLine ??= new VMMapLine( trackline.GetLinePoints( 0,0 ),Zoom,Clipping )
        {
          LineColor = file.LineColor,
          LineThickness = file.LineThickness,
        };
        trackline.MapLine?.SetZoomAndClipping( Zoom,Offset,Clipping );
        trackline.MapLine?.AddElements( Children );
      }
      Children.Add(new Line( ){ StartPoint = new Point(0,0),EndPoint = new Point(1000,1000),Stroke = Brushes.Black, StrokeThickness = 10 });
      
      file.Lines.SelectedItems.CollectionChanged -= CollectionChanged;
      file.Lines.SelectedItems.CollectionChanged += CollectionChanged;
    }
  }
}
