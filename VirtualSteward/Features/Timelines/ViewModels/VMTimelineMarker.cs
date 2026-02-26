using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Avalonia;

using Framework.UI;

namespace VirtualSteward.Features.Timelines.ViewModels;

public partial class VMTimelineMarker( string title,double start ) : UIItem
{
    public string Title { get; } = title;

    public uint Start { get; set; }
    public uint End { get; set; }

    public uint StartFrame { get; init; }
    public uint EndFrame { get; init; }

    public double Position { get; } = start;
}

public class VMTimelineMarkerList : ObservableCollection<VMTimelineMarker>
{

}
