using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;

using Framework.UI;

using VirtualSteward.Classes;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.PlayersLabels.ViewModels;
using VirtualSteward.Features.PlayersLabels.Configurations;

namespace VirtualSteward.Features.PlayersLabels;

public class PlayersLabels : StateFeature
{
    private readonly VMTimeline _timeline;
    private readonly VMPlayersLabelsLayer _labelsLayer;

    public PlayersLabelsOptions Options { get; }
    public ObservableCollection<VMPlayer> Players { get; }

    public PlayersLabels( State state,DataTemplates? templates,VMMap map,VMTimeline timeline,VMPlayerList players ) : base( state,templates,null,timeline )
    {
        _timeline = timeline;
        
        map.Layers.Add( _labelsLayer = new VMPlayersLabelsLayer( Players = players.VisibleItems ) { IsVisible = false } );

        AddConfiguration( Options = new PlayersLabelsOptions( state.GetPlayerLabelStyle(  ),this ) );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<VMPlayersLabelsLayer>( ( _,_ ) => new Controls.PlayersLabels( ) ) );
        templates.Add( new FuncDataTemplate<PlayersLabelsOptions>( ( _,_ ) => new Framework.UI.Panels.BaseConfiguration( ) ) );

        return this;
    }
    public override Feature AddFooter( UIBaseList pages,string? headerTitle = null )
    {
        pages.Add( Options );
        
        return this;
    }

    public override void OnTimelineChange( VMTimeline timeline,TimelineChangeType type )
    {
        if( type == TimelineChangeType.IsActive )
            UpdateVisibility( );
    }
    
    public void UpdateVisibility( )
    {
        _labelsLayer.IsVisible = _timeline.IsActive && Options.LabelsVisible.Value;
    }
}