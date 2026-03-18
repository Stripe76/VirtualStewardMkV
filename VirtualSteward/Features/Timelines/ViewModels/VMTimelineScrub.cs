using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Timelines.ViewModels;

public abstract partial class VMTimelineScrub : UIBase
{
    protected readonly VMTimeline _timeline;

    public uint Frame { get; set; }
    public Point Position { get; set; } = new Point( 0,0 );
    public ICommand? PointerPressed { get; init; }

    [ObservableProperty] private VMMapItem _mapItem;

    public VMTimelineScrub( VMTimeline timeline )
    {
        _timeline = timeline;
        _mapItem = new VMMapItem( this );
    }

    public abstract void SetTimelineFrame( uint frame );
}

public class VMTimelineScrubA( VMTimeline timeline ) : VMTimelineScrub( timeline )
{
    public override void SetTimelineFrame( uint frame )
    {
        _timeline.ScrubA = frame;
    }
}

public class VMTimelineScrubB( VMTimeline timeline ) : VMTimelineScrub( timeline )
{
    public override void SetTimelineFrame( uint frame )
    {
        _timeline.ScrubB = frame;
    }
}

public class VMTimelineScrubList : ObservableCollectionEx<VMTimelineScrub>
{
    
}