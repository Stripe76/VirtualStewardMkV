using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.Settings;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.TrackMap;

public class TrackMap : Feature
{
  public VMMap Map { get; } = new VMMap( true );
  public VMMapLayerList Layers { get; } = [];
  
  public TrackMap( DataTemplates templates ) : base(templates)
  {
  }

  public void AddLayer(VMMapLayerNew layer)
  {
    Map.NewLayers.Add(layer);
  }

  public override Feature AddDataTemplates( DataTemplates templates )
  {
    templates.Add( new FuncDataTemplate<VMMap>( (_,_) => new Controls.MapDisplay( ) ) );
    templates.Add( new FuncDataTemplate<VMMapLinesLayer>( (_,_) => new Controls.MapLines( ) ) );

    return this;
  }
}
