using Avalonia;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public class VMMapImagesLayer(VMMapImageList images) : VMMapLayerNew
{
    public VMMapImageList Images { get; } = images;

    public override void UpdateLayer(double zoom, Point offset, Rect clipping)
    {
    }
}