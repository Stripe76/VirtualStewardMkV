using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls.Templates;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.PlayersLines.Configurations;

namespace VirtualSteward.Features.PlayersLines;

public class PlayersLines : StateFeature
{
    private readonly VMMap _map;
    private readonly VMTimeline _timeline;
    private readonly VMMapLineList _lines  = [];
    private readonly VMMapLinesLayer _linesLayer; 
    private readonly ObservableCollection<VMPlayer> _players;
    
    public PlayersLinesOptions Options { get; }

    public PlayersLines( State state,DataTemplates? templates,VMMap map,VMTimeline timeline,VMPlayerList players ) : base( state,templates,null,timeline )
    {
        _map = map;
        _timeline = timeline;
        _players = players.SelectedItems;
        _players.CollectionChanged += SelectedPlayers_CollectionChanged;

        UpdateLines( );

        map.Layers.Add( _linesLayer = new VMMapLinesLayer( _lines ) );

        Options = new PlayersLinesOptions( this );
        Options.LinesVisible.Value = true;
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<PlayersLinesOptions>( ( _,_ ) => new Framework.UI.Panels.BaseConfiguration( ) ) );

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
        _linesLayer.IsVisible = _timeline.IsActive && Options.LinesVisible.Value;
    }

    private void UpdateLines()
    {
        _lines.Clear();

        foreach( var player in _players )
        {
            foreach( var lap in player.Laps.SelectedItems )
            {
                lap.Lines ??= GenerateLapLines( _map,player,lap );

                foreach( var line in lap.Lines )
                {
                    _lines.Add( line.UpdatePolylines( _map.Zoom,_map.Offset,_map.Clipping ) );
                }
            }
            player.Laps.SelectedItems.CollectionChanged -= SelectedLaps_CollectionChanged;
            player.Laps.SelectedItems.CollectionChanged += SelectedLaps_CollectionChanged;
        }
    }

    private static VMMapLineList GenerateLapLines(VMMap map,VMPlayer player,VMPlayerLap lap)
    {
        VMMapLineList lines =
        [
            new VMMapLine( player.GetLineSegment(lap.StartFrame, lap.EndFrame),player.LineStyle )
        ];
        var brakings = player.GetBrakingLineSegment(lap.StartFrame, lap.EndFrame);
        foreach (var line in brakings)
        {
            lines.Add(new VMMapLine(line, player.LineStyle));
        }
        return lines;
    }

    private void SelectedLaps_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLines( );
    }
    private void SelectedPlayers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLines( );
    }
}
