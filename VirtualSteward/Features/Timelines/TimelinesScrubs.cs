using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Timelines.EditingTools;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Timelines;

public partial class TimelinesScrubs : StateFeature
{
    private VMMap _map;

    private VMPlayer? _player;
    private VMTimelineScrub _scrubA,_scrubB;
    private VMTimelineScrubList _scrubs { get; } = [];
    private VMTimelineScrubLayer _layer;

    public TimelinesScrubs( State state,DataTemplates templates,VMPlayerList players,VMMap map,VMTimeline timeline ) : base( state,templates,map,timeline )
    {
        _map = map;
            
        _scrubs.Add( _scrubA = new VMTimelineScrubA( timeline )
        {
            PointerPressed = ScrubSelectedCommand 
        });
        _scrubs.Add( _scrubB = new VMTimelineScrubB( timeline )       
        {
            PointerPressed = ScrubSelectedCommand 
        });
        map.AddLayer( _layer = new VMTimelineScrubLayer( _scrubs ) );

        players.ActiveItemChanged += Players_ActiveItemChanged; 
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<VMTimelineScrub>( ( _,_ ) => new Controls.TimelineScrub( ) ) );
        templates.Add( new FuncDataTemplate<VMTimelineScrubLayer>( ( _,_ ) => new TrackMap.Controls.MapItems( ) ) );

        return this;
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        _layer.IsVisible = replay.IsLoaded;
    }
    public override void OnMapChange( VMMap map )
    {
        UpdateScrubs( map );
    }
    public override void OnTimelineChange( VMTimeline timeline,TimelineChangeType type )
    {
        if( type == TimelineChangeType.Scrubs )
        {
            _scrubA.Frame = timeline.ScrubA;
            _scrubB.Frame = timeline.ScrubB;
            
            UpdateScrubs( _map );
        }
        else if( type == TimelineChangeType.IsActive )
        {
            _layer.IsVisible = timeline.IsActive;
        }
    }
    
    private void UpdateScrubs( VMMap map )
    {
        if( _player != null )
        {
            foreach( var scrub in _scrubs )
            {
                var position = _player.Datasource.GetPositionAndRotation( scrub.Frame );
                if( position != null )
                {
                    scrub.MapItem.Position = map.TrackToCanvas( position.Position.X,position.Position.Y );
                    //checkpoint.MapItem.Scale = _map.Zoom;
                    //checkpoint.MapItem.Rotation = Mathematics.Degrees( pos.Rotation.X );
                }
            }
        }
    }

    [RelayCommand]
    protected void ScrubSelected( VMTimelineScrub scrub )
    {
        if( _player != null )
            _map.EditingTool = new TimelineScrubEdit( _player,scrub );
    }

    private void Players_ActiveItemChanged( object? sender,VMPlayer? e )
    {
        if( e != null )
            _player = e;
        UpdateScrubs( _map );
    }
}