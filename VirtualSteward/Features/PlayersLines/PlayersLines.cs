using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Avalonia.Controls.Templates;

using Framework.UI;

using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersLines;

public class PlayersLines : Feature
{
    private readonly VMMap _map;
    private readonly VMMapLineNewList _lines  = [];
    private readonly ObservableCollection<VMPlayer> _players;
    
    public PlayersLines(DataTemplates templates,VMMap map,VMPlayerList players) : base(templates)
    {
        _map = map;
        _players = players.SelectedItems;
        _players.CollectionChanged += SelectedPlayers_CollectionChanged;
        
        UpdateLines( );

        map.Layers.Add( new VMMapLinesLayer( _lines ) );
    }

    private void UpdateLines()
    {
        _lines.Clear();
        
        foreach (var player in _players)
        {
            foreach (var lap in player.Laps.SelectedItems)
            {
                lap.Lines ??= GenerateLapLines(_map, player, lap);

                foreach (var line in lap.Lines )
                {
                    _lines.Add(line.UpdatePolylines(_map.Zoom,_map.Offset,_map.Clipping));
                }
            }
            player.Laps.SelectedItems.CollectionChanged -= SelectedLaps_CollectionChanged;
            player.Laps.SelectedItems.CollectionChanged += SelectedLaps_CollectionChanged;
        }
    }

    private static VMMapLineNewList GenerateLapLines(VMMap map,VMPlayer player,VMPlayerLap lap)
    {
        VMMapLineNewList lines =
        [
            new VMMapLineNew( player.GetLineSegment(lap.StartFrame, lap.EndFrame),player.LineStyle )
        ];
        var brakings = player.GetBrakingLineSegment(lap.StartFrame, lap.EndFrame);
        foreach (var line in brakings)
        {
            lines.Add(new VMMapLineNew(line, player.LineStyle));
        }
        return lines;
    }

    private void SelectedPlayers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLines( );
    }
    private void SelectedLaps_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLines( );
    }
}
