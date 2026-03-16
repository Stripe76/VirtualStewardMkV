using Avalonia;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public class VMMapLinesLayer( VMMapLineNewList lines ) : VMMapLayer
{
    public VMMapLineNewList Lines { get; } = lines;

    public override void UpdateLayer( double zoom,Point offset,Rect clipping )
    {
        foreach( var line in Lines )
        {
            line.UpdatePolylines( zoom,offset,clipping );
        }
    }
}