using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Framework.Bindables;
using Framework.UI.Values;

using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Values;

public class TimelineExportValue : BaseValue<TimelineExportItem>
{
  private readonly VMTimelineList _timelines;

  public TimelineExportItemList Items
  {
    get;
  } = [];

  public TimelineExportValue( VMTimelineList timelines ) : base( null,"Timeline","" )
  {
    _timelines = timelines;
    _timelines.CollectionChanged += Timelines_CollectionChanged;

    UpdateItems( );
  }

  private void UpdateItems( )
  {
    foreach( var timeline in _timelines )
    {
      Items.Add( new( timeline,timeline.TimelineName ) );
      Items.Add( new( timeline,timeline.TimelineName,true ) );
    }
    if( Items.Count > 0 )
      Value = Items[0];
  }

  private void Timelines_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( sender != null && sender == _timelines )
    {
      if( e.Action is NotifyCollectionChangedAction.Add or 
                      NotifyCollectionChangedAction.Remove or 
                      NotifyCollectionChangedAction.Reset )
      {
        Items.Clear( );

        UpdateItems( );
      }
    }
  }
}

public class TimelineExportItem : BindableBase
{
  private readonly string _title;

  public string Title => $"{_title}{GetSuffix( )}";

  public bool OnlySegment { get; }
  public VMTimeline Timeline { get; }

  public TimelineExportItem( VMTimeline timeline,string title,bool segment = false )
  {
    _title = title;
    
    OnlySegment = segment;

    Timeline = timeline;
    Timeline.PropertyChanged += Timeline_PropertyChanged;
  }

  private void Timeline_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
  {
    if( e.PropertyName == "ScrubA" || e.PropertyName == "ScrubB" )
      OnPropertyChanged( nameof( Title ) );
  }

  private string GetSuffix( )
  {
    if( OnlySegment )
      return $" - Segment ({Timeline.ScrubA}-{Timeline.ScrubB})";
    return " - Whole";
  }
}

public class TimelineExportItemList : ObservableCollection<TimelineExportItem>
{
}