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

    private void InputElement_OnPointerPressed( object? sender,PointerPressedEventArgs e )
    {
        if( DataContext is not null and VMTimeline timeline )
            timeline.IsActive = true;
    }
}