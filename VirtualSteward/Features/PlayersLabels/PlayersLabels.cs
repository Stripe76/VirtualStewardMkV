using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;
using Framework.UI;
using Framework.UI.Values;
using VirtualSteward.Classes;
using VirtualSteward.Features.PlayersLabels.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersLabels;

public class PlayersLabels : StateFeature
{
    private readonly VMPlayersLabelsLayer _labelsLayer;

    public BaseBool LabelsVisible { get; }
    public ObservableCollection<VMPlayer> Players { get; }

    public PlayersLabels( State state,DataTemplates? templates,VMMap map,VMTimeline timeline,VMPlayerList players ) : base( state,templates )
    {
        map.Layers.Add( _labelsLayer = new VMPlayersLabelsLayer( Players = players.VisibleItems ) );

        LabelsVisible = new BaseBool( "LABELS_VISIBLE","Players names" )
        {
            ValueChanged = ( value ) => _labelsLayer.IsVisible = value
        };
        LabelsVisible.Value = _labelsLayer.IsVisible;
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<VMPlayersLabelsLayer>( ( _,_ ) => new Controls.PlayersLabels( ) ) );

        return this;
    }

    public override Feature AddFooter( UIBaseList pages,string? headerTitle = null )
    {
        pages.Add( LabelsVisible );
        
        return this;
    }
}