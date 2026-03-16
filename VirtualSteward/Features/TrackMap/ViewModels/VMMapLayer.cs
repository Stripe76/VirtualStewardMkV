using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Bindables;
using Framework.UI;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public partial class VMMapLayer : UIBase
{
    [ObservableProperty] private double _zoom = 1.0f;
    [ObservableProperty] private Point _offset = new ( );
    [ObservableProperty] private Rect _clipping = new ( );

    public virtual void UpdateLayer( double zoom, Point offset,Rect clipping )
    {
        
    }
}

public class VMMapLayerList : ObservableCollectionEx<VMMapLayer>
{
    
}