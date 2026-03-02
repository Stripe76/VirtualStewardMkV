using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;

using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersList;

public partial class PlayersList( DataTemplates templates ) : Feature(templates)
{
    public override void AddDataTemplates(DataTemplates templates)
    {
        templates.Add( new FuncDataTemplate<VMPlayer>( (_,_) => new Controls.Player() ) );
        templates.Add( new FuncDataTemplate<VMPlayerLapList>( (_,_) => new Controls.PlayerLaps() ) );
        templates.Add( new FuncDataTemplate<VMPlayerInfoEditing>( (_,_) => new Controls.PlayerInfoEditing() ) );
    }
}