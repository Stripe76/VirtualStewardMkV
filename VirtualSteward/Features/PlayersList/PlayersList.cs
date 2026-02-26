using Avalonia.Controls.Templates;

using Framework.UI;

using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersList;

public class PlayersList : Feature
{
    public PlayersList( DataTemplates templates,TrackMap.TrackMap map,VMPlayerList players ) : base(templates)
    {
        //map.Map.AddLayer(new VMLayerPlayersLaps( players.SelectedItems ));
    }

    public override void AddDataTemplates(DataTemplates templates)
    {
        templates.Add( new FuncDataTemplate<VMPlayer>( (_,_) => new Controls.Player() ) );
        templates.Add( new FuncDataTemplate<VMPlayerLapList>( (_,_) => new Controls.PlayerLaps() ) );
    }
}