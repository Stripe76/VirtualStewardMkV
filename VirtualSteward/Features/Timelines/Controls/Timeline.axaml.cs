using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.Timelines.Controls;

public partial class Timeline : UserControl
{
    public Timeline()
    {
        InitializeComponent();
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is not null and VMTimeline timeline)
        {
            foreach (var marker in timeline.Markers)
            { 
                //marker.Margin = e.NewSize.Width * marker.Position;
                //marker.Margin = new Thickness( Bounds.Width * marker.Position,0,0,0 );
            }
        }
    }
    private void Marker_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not null and VMTimeline timeline)
        {
            if (sender is not null and Visual { DataContext: not null and VMTimelineMarker marker })
            {
                timeline.ScrubA = marker.StartFrame;
                timeline.ScrubB = marker.EndFrame;
            }
        }
    }
}