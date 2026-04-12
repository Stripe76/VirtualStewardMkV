using System.Collections.ObjectModel;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersLabels.ViewModels;

public class VMPlayersLabelsLayer(ObservableCollection<VMPlayer> players) : VMMapLayer
{
    public ObservableCollection<VMPlayer> Players { get; } = players;
}